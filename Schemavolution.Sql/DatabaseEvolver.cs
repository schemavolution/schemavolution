using Schemavolution.Sql.Loader;
using Schemavolution.Specification;
using Schemavolution.Specification.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Schemavolution.Sql
{
    public class DatabaseEvolver
    {
        private readonly string _databaseName;
        private readonly string _fileName;
        private readonly string _masterConnectionString;
        private readonly IGenome _genome;

        public DatabaseEvolver(string databaseName, string fileName, string masterConnectionString, IGenome genome)
        {
            _databaseName = databaseName;
            _fileName = fileName;
            _masterConnectionString = masterConnectionString;
            _genome = genome;
        }

        public DatabaseEvolver(string databaseName, string masterConnectionString, IGenome genome)
        {
            _databaseName = databaseName;
            _fileName = null;
            _masterConnectionString = masterConnectionString;
            _genome = genome;
        }

        public bool EvolveDatabase()
        {
            var evolutionHistory = LoadEvolutionHistory();

            if (evolutionHistory.Empty)
            {
                string[] initialize =
                {
                    _fileName != null ?
                    $@"CREATE DATABASE [{_databaseName}]
                        ON (NAME = '{_databaseName}',
                        FILENAME = '{_fileName}')" :
                    $"CREATE DATABASE [{_databaseName}]",
                    $@"CREATE TABLE [{_databaseName}].[dbo].[__EvolutionHistory](
                        [GeneId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Type] VARCHAR(50) NOT NULL,
                        [HashCode] VARBINARY(32) NOT NULL,
                        [Attributes] NVARCHAR(MAX) NOT NULL,
	                    INDEX [IX_EvolutionHistory_HashCode] UNIQUE ([HashCode]))",
                    $@"CREATE TABLE [{_databaseName}].[dbo].[__EvolutionHistoryPrerequisite] (
	                    [GeneId] INT NOT NULL,
	                    [Role] NVARCHAR(50) NOT NULL,
                        [Ordinal] INT NOT NULL,
	                    [PrerequisiteGeneId] INT NOT NULL,
	                    INDEX [IX_EvolutionHistoryPrerequisite_GeneId] ([GeneId]),
	                    FOREIGN KEY ([GeneId]) REFERENCES [{_databaseName}].[dbo].[__EvolutionHistory],
	                    INDEX [IX_EvolutionHistoryPrerequisite_PrerequisiteMigrationId] ([PrerequisiteGeneId]),
	                    FOREIGN KEY ([PrerequisiteGeneId]) REFERENCES [{_databaseName}].[dbo].[__EvolutionHistory])"
                };
                ExecuteSqlCommands(initialize);
            }

            var generator = new SqlGenerator(_genome, evolutionHistory);

            var sql = generator.Generate(_databaseName);
            ExecuteSqlCommands(sql);

            return sql.Any();
        }

        public bool DevolveDatabase()
        {
            var evolutionHistory = LoadEvolutionHistory();
            var generator = new SqlGenerator(_genome, evolutionHistory);
            var sql = generator.GenerateRollbackSql(_databaseName);

            ExecuteSqlCommands(sql);

            return sql.Any();
        }

        public void DestroyDatabase()
        {
            var fileNames = ExecuteSqlQuery($@"
                SELECT [physical_name] FROM [sys].[master_files]
                WHERE [database_id] = DB_ID('{_databaseName}')",
                row => (string)row["physical_name"]);

            if (fileNames.Any())
            {
                ExecuteSqlCommand($@"
                    ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    EXEC sp_detach_db '{_databaseName}'");

                fileNames.ForEach(File.Delete);
            }
        }

        private EvolutionHistory LoadEvolutionHistory()
        {
            var ids = ExecuteSqlQuery($"SELECT database_id FROM master.sys.databases WHERE name = '{_databaseName}'",
                row => (int)row["database_id"]);
            if (ids.Any())
            {
                string upgrade = $@"IF COL_LENGTH('[{_databaseName}].[dbo].[__EvolutionHistoryPrerequisite]', 'Ordinal') IS NULL
                    BEGIN
	                    ALTER TABLE [{_databaseName}].[dbo].[__EvolutionHistoryPrerequisite]
	                    ADD [Ordinal] INT NOT NULL
	                    CONSTRAINT [DF_EvolutionHistoryPrerequisite_Ordinal] DEFAULT (1)

	                    ALTER TABLE [{_databaseName}].[dbo].[__EvolutionHistoryPrerequisite]
                        DROP CONSTRAINT [DF_EvolutionHistoryPrerequisite_Ordinal]
                    END";
                ExecuteSqlCommand(upgrade);

                var rows = ExecuteSqlQuery($@"SELECT h.[Type], h.[HashCode], h.[Attributes], j.[Role], p.[HashCode] AS [PrerequisiteHashCode]
                        FROM [{_databaseName}].[dbo].[__EvolutionHistory] h
                        LEFT JOIN [{_databaseName}].[dbo].[__EvolutionHistoryPrerequisite] j
                          ON h.GeneId = j.GeneId
                        LEFT JOIN [{_databaseName}].[dbo].[__EvolutionHistory] p
                          ON j.PrerequisiteGeneId = p.GeneId
                        ORDER BY h.GeneId, j.Role, j.Ordinal, p.GeneId",
                    row => new EvolutionHistoryRow
                    {
                        Type = LoadString(row["Type"]),
                        HashCode = LoadBigInteger(row["HashCode"]),
                        Attributes = LoadString(row["Attributes"]),
                        Role = LoadString(row["Role"]),
                        PrerequisiteHashCode = LoadBigInteger(row["PrerequisiteHashCode"])
                    });

                return EvolutionHistory.LoadMementos(LoadMementos(rows));
            }
            else
            {
                return new EvolutionHistory();
            }
        }

        private static IEnumerable<GeneMemento> LoadMementos(
            IEnumerable<EvolutionHistoryRow> rows)
        {
            var enumerator = new LookaheadEnumerator<EvolutionHistoryRow>(rows.GetEnumerator());
            enumerator.MoveNext();
            if (enumerator.More)
            {
                do
                {
                    yield return LoadMemento(enumerator);
                } while (enumerator.More);
            }
        }

        private static GeneMemento LoadMemento(LookaheadEnumerator<EvolutionHistoryRow> enumerator)
        {
            var type = enumerator.Current.Type;
            var hashCode = enumerator.Current.HashCode;
            var attributes = enumerator.Current.Attributes;
            var roles = LoadRoles(hashCode, enumerator);

            var geneAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(attributes);
            var memento = new GeneMemento(
                type,
                geneAttributes,
                hashCode,
                roles);
            return memento;
        }

        private static IDictionary<string, IEnumerable<BigInteger>> LoadRoles(BigInteger hashCode, LookaheadEnumerator<EvolutionHistoryRow> enumerator)
        {
            var result = new Dictionary<string, IEnumerable<BigInteger>>();
            do
            {
                string role = enumerator.Current.Role;
                if (role != null)
                {
                    var prerequisites = LoadPrerequisites(hashCode, role, enumerator).ToList();
                    result[role] = prerequisites;
                }
                else
                {
                    enumerator.MoveNext();
                }
            } while (enumerator.More && enumerator.Current.HashCode == hashCode);

            return result;
        }

        private static IEnumerable<BigInteger> LoadPrerequisites(BigInteger hashCode, string role, LookaheadEnumerator<EvolutionHistoryRow> enumerator)
        {
            do
            {
                yield return enumerator.Current.PrerequisiteHashCode;
                enumerator.MoveNext();
            } while (enumerator.More && enumerator.Current.HashCode == hashCode && enumerator.Current.Role == role);
        }

        private static string LoadString(object value)
        {
            if (value is DBNull)
                return null;
            else
                return (string)value;
        }

        private static BigInteger LoadBigInteger(object value)
        {
            if (value is DBNull)
                return BigInteger.Zero;
            else
                return new BigInteger(((byte[])value).Reverse().ToArray());
        }

        private void ExecuteSqlCommand(string commandText)
        {
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ExecuteSqlCommands(IEnumerable<string> commands)
        {
            if (commands.Any())
            {
                using (var connection = new SqlConnection(_masterConnectionString))
                {
                    connection.Open();
                    foreach (var commandText in commands)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = commandText;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private List<T> ExecuteSqlQuery<T>(
            string queryText,
            Func<SqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryText;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(read(reader));
                        }
                    }
                }
            }
            return result;
        }
    }
}

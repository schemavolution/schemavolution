using Schemavolution.Evolve.Loader;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Schemavolution.Evolve.Executor
{
    class SqlServerExecutor : IDatabaseExecutor
    {
        private readonly string _masterConnectionString;

        public SqlServerExecutor(string masterConnectionString)
        {
            _masterConnectionString = masterConnectionString;
        }

        public bool DatabaseExists(string databaseName)
        {
            var ids = ExecuteSqlQuery($"SELECT database_id FROM master.sys.databases WHERE name = '{databaseName}'",
                row => (int)row["database_id"]);
            bool exist = ids.Any();
            return exist;
        }

        public void UpgradeDatabase(string databaseName)
        {
            string upgrade = $@"IF COL_LENGTH('[{databaseName}].[dbo].[__EvolutionHistoryPrerequisite]', 'Ordinal') IS NULL
                    BEGIN
	                    ALTER TABLE [{databaseName}].[dbo].[__EvolutionHistoryPrerequisite]
	                    ADD [Ordinal] INT NOT NULL
	                    CONSTRAINT [DF_EvolutionHistoryPrerequisite_Ordinal] DEFAULT (1)

	                    ALTER TABLE [{databaseName}].[dbo].[__EvolutionHistoryPrerequisite]
                        DROP CONSTRAINT [DF_EvolutionHistoryPrerequisite_Ordinal]
                    END";
            ExecuteSqlCommand(upgrade);
        }

        public void DestroyDatabase(string databaseName)
        {
            var fileNames = ExecuteSqlQuery($@"
                SELECT [physical_name] FROM [sys].[master_files]
                WHERE [database_id] = DB_ID('{databaseName}')",
                row => (string)row["physical_name"]);

            if (fileNames.Any())
            {
                ExecuteSqlCommand($@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    EXEC sp_detach_db '{databaseName}'");

                fileNames.ForEach(File.Delete);
            }
        }

        public List<EvolutionHistoryRow> LoadEvolutionHistory(string databaseName)
        {
            return ExecuteSqlQuery($@"SELECT h.[Type], h.[HashCode], h.[Attributes], j.[Role], p.[HashCode] AS [PrerequisiteHashCode]
                        FROM [{databaseName}].[dbo].[__EvolutionHistory] h
                        LEFT JOIN [{databaseName}].[dbo].[__EvolutionHistoryPrerequisite] j
                          ON h.GeneId = j.GeneId
                        LEFT JOIN [{databaseName}].[dbo].[__EvolutionHistory] p
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
        }

        public void ExecuteSqlCommands(IEnumerable<string> commands)
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

        private string LoadString(object value)
        {
            if (value is DBNull)
                return null;
            else
                return (string)value;
        }

        private BigInteger LoadBigInteger(object value)
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

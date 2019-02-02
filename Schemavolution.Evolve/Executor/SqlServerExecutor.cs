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
        private readonly string _databaseName;

        public SqlServerExecutor(string masterConnectionString, string databaseName)
        {
            _masterConnectionString = masterConnectionString;
            _databaseName = databaseName;
        }

        public bool DatabaseExists()
        {
            var ids = ExecuteSqlQuery($"SELECT database_id FROM master.sys.databases WHERE name = '{_databaseName}'",
                row => (int)row["database_id"]);
            bool exist = ids.Any();
            return exist;
        }

        public void CreateDatabase(string fileName)
        {
            var sql = new string[]{
                fileName != null ?
                $@"CREATE DATABASE [{_databaseName}]
                        ON (NAME = '{_databaseName}',
                        FILENAME = '{fileName}')" :
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
            ExecuteSqlCommands(sql);
        }

        public void UpgradeDatabase()
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

        public List<EvolutionHistoryRow> LoadEvolutionHistory()
        {
            return ExecuteSqlQuery($@"SELECT h.[Type], h.[HashCode], h.[Attributes], j.[Role], p.[HashCode] AS [PrerequisiteHashCode]
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
        }

        public void ExecuteSqlCommands(IEnumerable<string> commands)
        {
            if (commands.Any())
            {
                using (var connection = new SqlConnection(_masterConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var commandText in commands)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = commandText;
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
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

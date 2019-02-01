using System;
using System.Collections.Generic;
using Schemavolution.Evolve.Loader;
using Npgsql;
using System.Linq;
using System.Numerics;

namespace Schemavolution.Evolve.Executor
{
    class PostgreSqlExecutor : IDatabaseExecutor
    {
        private string _masterConnectionString;
        private string _databaseName;
        private string _databaseConnectionString;

        public PostgreSqlExecutor(string masterConnectionString, string databaseName)
        {
            _masterConnectionString = masterConnectionString;
            _databaseName = databaseName;

            var builder = new NpgsqlConnectionStringBuilder(masterConnectionString);
            builder.Database = databaseName;
            _databaseConnectionString = builder.ConnectionString;
        }

        public bool DatabaseExists()
        {
            var names = ExecuteSqlQuery(
                _masterConnectionString,
                $"SELECT datname from pg_database WHERE datname='{_databaseName}';",
                row => (string)row["datname"]);
            bool exist = names.Any();
            return exist;
        }

        public void CreateDatabase(string fileName)
        {
            using (var connection = new NpgsqlConnection(_masterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"CREATE DATABASE ""{_databaseName}"";";
                    command.ExecuteNonQuery();
                }
            }

            ExecuteSqlCommands(new string[]
            {
                $@"CREATE TABLE ""__evolution_history"" (
                    ""gene_id"" serial PRIMARY KEY,
                    ""type"" varchar(50) NOT NULL,
                    ""hash_code"" bit varying(256) NOT NULL UNIQUE,
                    ""attributes"" jsonb
                );",
                @"CREATE TABLE ""__evolution_history_prerequisite"" (
                    ""gene_id"" int NOT NULL REFERENCES ""__evolution_history"",
                    ""role"" varchar(50) NOT NULL,
                    ""ordinal"" int NOT NULL,
                    ""prerequisite_gene_id"" int NOT NULL REFERENCES ""__evolution_history""
                );",
                @"CREATE INDEX ""__evolution_history_prerequisite_gene_id_idx""
                    ON ""__evolution_history_prerequisite""(""gene_id"");",
                @"CREATE INDEX ""__evolution_history_prerequisite_prerequisite_gene_id_idx""
                    ON ""__evolution_history_prerequisite""(""prerequisite_gene_id"");"
            });
        }

        public void DestroyDatabase()
        {
            throw new System.NotImplementedException("DestroyDatabase");
        }

        public void ExecuteSqlCommands(IEnumerable<string> commands)
        {
            if (commands.Any())
            {
                using (var connection = new NpgsqlConnection(_databaseConnectionString))
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

        public List<EvolutionHistoryRow> LoadEvolutionHistory()
        {
            return ExecuteSqlQuery(
                _databaseConnectionString,
                $@"SELECT h.""type"", h.""hash_code"", h.""attributes"", j.""role"", p.""hash_code"" AS ""prerequisite_hash_code""
                    FROM ""public"".""__evolution_history"" h
                    LEFT JOIN ""public"".""__evolution_history_prerequisite"" j
                        ON h.gene_id = j.gene_id
                    LEFT JOIN ""public"".""__evolution_history"" p
                        ON j.prerequisite_gene_id = p.gene_id
                    ORDER BY h.gene_id, j.role, j.ordinal, p.gene_id;",
                row => new EvolutionHistoryRow
                {
                    Type = LoadString(row["type"]),
                    HashCode = LoadBigInteger(row["hash_code"]),
                    Attributes = LoadString(row["attributes"]),
                    Role = LoadString(row["role"]),
                    PrerequisiteHashCode = LoadBigInteger(row["prerequisite_hash_code"])
                });
        }

        public void UpgradeDatabase()
        {
        }

        private List<T> ExecuteSqlQuery<T>(
            string connectionString,
            string queryText,
            Func<NpgsqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new NpgsqlConnection(connectionString))
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
    }
}
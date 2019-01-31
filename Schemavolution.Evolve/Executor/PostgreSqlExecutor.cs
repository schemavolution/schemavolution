using System;
using System.Collections.Generic;
using Schemavolution.Evolve.Loader;
using Npgsql;
using System.Linq;

namespace Schemavolution.Evolve.Executor
{
    class PostgreSqlExecutor : IDatabaseExecutor
    {
        private string _masterConnectionString;

        public PostgreSqlExecutor(string masterConnectionString)
        {
            _masterConnectionString = masterConnectionString;
        }

        public bool DatabaseExists(string databaseName)
        {
            var names = ExecuteSqlQuery($"SELECT datname from pg_database WHERE datname='{databaseName}';",
                row => (string)row["datname"]);
            bool exist = names.Any();
            return exist;
        }

        public void DestroyDatabase(string databaseName)
        {
            throw new System.NotImplementedException("DestroyDatabase");
        }

        public void ExecuteSqlCommands(IEnumerable<string> commands)
        {
            throw new System.NotImplementedException("ExecuteSqlCommands");
        }

        public List<EvolutionHistoryRow> LoadEvolutionHistory(string databaseName)
        {
            throw new System.NotImplementedException("LoadEvolutionHistory");
        }

        public void UpgradeDatabase(string databaseName)
        {
            throw new System.NotImplementedException("UpgradeDatabase");
        }

        private List<T> ExecuteSqlQuery<T>(
            string queryText,
            Func<NpgsqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new NpgsqlConnection(_masterConnectionString))
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
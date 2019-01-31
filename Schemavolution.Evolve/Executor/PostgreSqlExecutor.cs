using System.Collections.Generic;
using Schemavolution.Evolve.Loader;

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
            throw new System.NotImplementedException();
        }

        public void DestroyDatabase(string databaseName)
        {
            throw new System.NotImplementedException();
        }

        public void ExecuteSqlCommands(IEnumerable<string> commands)
        {
            throw new System.NotImplementedException();
        }

        public List<EvolutionHistoryRow> LoadEvolutionHistory(string databaseName)
        {
            throw new System.NotImplementedException();
        }

        public void UpgradeDatabase(string databaseName)
        {
            throw new System.NotImplementedException();
        }
    }
}
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
            throw new System.NotImplementedException("DatabaseExists");
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
    }
}
using System.Collections.Generic;
using Schemavolution.Evolve.Loader;

namespace Schemavolution.Evolve.Executor
{
    interface IDatabaseExecutor
    {
        bool DatabaseExists(string databaseName);
        void UpgradeDatabase(string databaseName);
        void DestroyDatabase(string databaseName);
        List<EvolutionHistoryRow> LoadEvolutionHistory(string databaseName);
        void ExecuteSqlCommands(IEnumerable<string> commands);
    }
}
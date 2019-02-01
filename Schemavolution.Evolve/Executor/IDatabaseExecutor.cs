using System.Collections.Generic;
using Schemavolution.Evolve.Loader;

namespace Schemavolution.Evolve.Executor
{
    interface IDatabaseExecutor
    {
        bool DatabaseExists();
        void CreateDatabase(string fileName);
        void UpgradeDatabase();
        void DestroyDatabase();
        List<EvolutionHistoryRow> LoadEvolutionHistory();
        void ExecuteSqlCommands(IEnumerable<string> commands);
    }
}
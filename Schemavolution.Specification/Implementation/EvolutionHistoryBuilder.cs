using System.Collections.Generic;

namespace Schemavolution.Specification.Implementation
{
    public class EvolutionHistoryBuilder
    {
        public EvolutionHistory EvolutionHistory { get; private set; } =
            new EvolutionHistory();

        public void Append(Gene gene)
        {
            EvolutionHistory = EvolutionHistory.Add(gene);
        }

        public void AppendAll(IEnumerable<Gene> genes)
        {
            EvolutionHistory = EvolutionHistory.AddAll(genes);
        }
    }
}

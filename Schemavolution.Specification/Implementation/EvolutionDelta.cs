using System;
using System.Collections.Immutable;
using System.Linq;

namespace Schemavolution.Specification.Implementation
{
    public class EvolutionDelta
    {
        private readonly ImmutableList<Gene> _genes;

        internal EvolutionDelta(ImmutableList<Gene> genes)
        {
            _genes = genes;
        }

        public bool Any => _genes.Any();
        public Gene Head => _genes.First();

        public bool Contains(Gene gene)
        {
            return _genes.Contains(gene);
        }

        public EvolutionDelta Subtract(EvolutionHistory evolutionHistory)
        {
            return new EvolutionDelta(_genes.RemoveAll(x =>
                evolutionHistory.Contains(x)));
        }
    }
}

using Schemavolution.Specification.Implementation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Schemavolution.Specification
{
    public abstract class Specification
    {
        private readonly EvolutionHistoryBuilder _evolutionHistoryBuilder;
        private readonly ImmutableList<Gene> _prerequisites;

        protected EvolutionHistoryBuilder EvolutionHistoryBuilder => _evolutionHistoryBuilder;
        protected ImmutableList<Gene> Prerequisites => _prerequisites;
        internal abstract IEnumerable<Gene> Genes { get; }

        protected Specification(EvolutionHistoryBuilder evolutionHistoryBuilder)
        {
            _evolutionHistoryBuilder = evolutionHistoryBuilder;
            _prerequisites = ImmutableList<Gene>.Empty;
        }

        protected Specification(EvolutionHistoryBuilder evolutionHistoryBuilder, ImmutableList<Gene> prerequisites)
        {
            _evolutionHistoryBuilder = evolutionHistoryBuilder;
            _prerequisites = prerequisites;
        }
    }
}

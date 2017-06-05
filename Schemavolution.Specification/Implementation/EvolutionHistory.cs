using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace Schemavolution.Specification.Implementation
{
    public class EvolutionHistory
    {
        private readonly ImmutableList<Gene> _genes;
        private readonly ImmutableHashSet<Gene> _geneSet;

        public EvolutionHistory()
        {
            _genes = ImmutableList<Gene>.Empty;
            _geneSet = ImmutableHashSet<Gene>.Empty;
        }

        private EvolutionHistory(ImmutableList<Gene> genes, ImmutableHashSet<Gene> geneSet)
        {
            _genes = genes;
            _geneSet = geneSet;
        }

        public EvolutionHistory Add(Gene gene)
        {
            return new EvolutionHistory(
                _genes.Add(gene),
                _geneSet.Add(gene));
        }

        public EvolutionHistory AddAll(IEnumerable<Gene> genes)
        {
            return new EvolutionHistory(
                _genes.AddRange(genes),
                _geneSet.Union(genes));
        }

        public EvolutionDelta Subtract(EvolutionHistory evolutionHistory)
        {
            return new EvolutionDelta(_genes.RemoveAll(x =>
                evolutionHistory.Contains(x)));
        }

        public bool Empty =>
            !_genes.Any();

        public bool Contains(Gene gene) =>
            _geneSet.Contains(gene);

        public IEnumerable<GeneMemento> GetMementos() =>
            _genes.Select(m => m.GetMemento());

        public static EvolutionHistory LoadMementos(IEnumerable<GeneMemento> mementos)
        {
            var genes = ImmutableList<Gene>.Empty;
            var genesByHashCode = ImmutableDictionary<BigInteger, Gene>.Empty;

            foreach (var memento in mementos)
            {
                var gene = GeneLoader.Load(memento, genesByHashCode);
                if (gene.Sha256Hash != memento.HashCode)
                    throw new ArgumentException("Hash code does not match");
                genes = genes.Add(gene);
                gene.AddToParent();
                genesByHashCode = genesByHashCode
                    .Add(gene.Sha256Hash, gene);

            }

            var geneSet = genes.ToImmutableHashSet();

            return new EvolutionHistory(genes, geneSet);
        }
    }
}

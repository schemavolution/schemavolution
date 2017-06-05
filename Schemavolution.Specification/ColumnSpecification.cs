using System;
using System.Collections.Generic;
using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Linq;

namespace Schemavolution.Specification
{
    public class ColumnSpecification : Specification
    {
        private readonly CreateColumnGene _gene;

        internal CreateColumnGene Gene => _gene;
        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal ColumnSpecification(CreateColumnGene gene, EvolutionHistoryBuilder evolutionHistoryBuilder) :
            base(evolutionHistoryBuilder)
        {
            _gene = gene;
        }
    }
}
using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Collections.Generic;

namespace Schemavolution.Specification
{
    public class CustomSqlSpecification : Specification
    {
        private CustomSqlGene _gene;

        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal CustomSqlSpecification(CustomSqlGene gene, EvolutionHistoryBuilder migrationHistoryBuilder) :
            base(migrationHistoryBuilder)
        {
            _gene = gene;
        }
    }
}

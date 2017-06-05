using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Collections.Generic;

namespace Schemavolution.Specification
{
    public class IndexSpecification : Specification
    {
        private CreateIndexGene _gene;

        internal CreateIndexGene Gene => _gene;
        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal IndexSpecification(CreateIndexGene gene, EvolutionHistoryBuilder evolutionHistoryBuilder) :
            base(evolutionHistoryBuilder)
        {
            _gene = gene;
        }

        public ForeignKeySpecification CreateForeignKey(PrimaryKeySpecification referencing, bool cascadeDelete = false, bool cascadeUpdate = false)
        {
            var childGene = new CreateForeignKeyGene(
                _gene,
                referencing.Gene,
                cascadeDelete,
                cascadeUpdate,
                Prerequisites);
            EvolutionHistoryBuilder.Append(childGene);
            childGene.AddToParent();
            return new ForeignKeySpecification(EvolutionHistoryBuilder);
        }
    }
}
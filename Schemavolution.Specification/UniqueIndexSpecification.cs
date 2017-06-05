using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Schemavolution.Specification
{
    public class UniqueIndexSpecification : Specification
    {
        private CreateUniqueIndexGene _gene;

        internal CreateUniqueIndexGene Gene => _gene;
        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal UniqueIndexSpecification(CreateUniqueIndexGene gene, EvolutionHistoryBuilder geneHistoryBuilder) :
            base(geneHistoryBuilder)
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

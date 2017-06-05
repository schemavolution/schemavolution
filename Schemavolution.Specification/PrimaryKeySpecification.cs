using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Schemavolution.Specification
{
    public class PrimaryKeySpecification : Specification
    {
        private readonly CreatePrimaryKeyGene _gene;

        internal CreatePrimaryKeyGene Gene => _gene;
        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal PrimaryKeySpecification(CreatePrimaryKeyGene gene, EvolutionHistoryBuilder evolutionHistoryBuilder) :
            base(evolutionHistoryBuilder)
        {
            _gene = gene;
        }

        public ForeignKeySpecification CreateForeignKey(PrimaryKeySpecification referencing, bool cascadeDelete = false, bool cascadeUpdate = false)
        {
            var childGene = new CreateForeignKeyGene(
                _gene,
                referencing._gene,
                cascadeDelete,
                cascadeUpdate,
                Prerequisites);
            EvolutionHistoryBuilder.Append(childGene);
            childGene.AddToParent();
            return new ForeignKeySpecification(EvolutionHistoryBuilder);
        }
    }
}
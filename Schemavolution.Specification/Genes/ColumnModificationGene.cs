using Schemavolution.Specification.Implementation;
using System.Collections.Immutable;

namespace Schemavolution.Specification.Genes
{
    abstract class ColumnModificationGene : Gene
    {
        internal abstract CreateColumnGene CreateColumnGene { get; }

        protected ColumnModificationGene(ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
        }

        internal override void AddToParent()
        {
            CreateColumnGene.AddModification(this);
        }
    }
}

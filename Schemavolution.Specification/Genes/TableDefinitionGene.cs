using Schemavolution.Specification.Implementation;
using System.Collections.Immutable;

namespace Schemavolution.Specification.Genes
{
    abstract class TableDefinitionGene : Gene
    {
        internal abstract CreateTableGene CreateTableGene { get; }
        internal virtual bool Dropped => false;
        internal abstract string GenerateDefinitionSql();

        public TableDefinitionGene(ImmutableList<Gene> prerequisites)
            : base(prerequisites)
        {
        }

        internal override void AddToParent()
        {
            CreateTableGene.AddDefinition(this);
        }
    }
}

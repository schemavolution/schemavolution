using Schemavolution.Specification.Genes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Specification.Genes
{
    abstract class IndexGene : TableDefinitionGene
    {
        protected IndexGene(ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
        }

        public abstract string DatabaseName { get; }
        public abstract string SchemaName { get; }
        public abstract string TableName { get; }
        public abstract IEnumerable<CreateColumnGene> Columns { get; }
    }
}

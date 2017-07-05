using Schemavolution.Specification.Implementation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System;

namespace Schemavolution.Specification.Genes
{
    class DropColumnGene : ColumnModificationGene
    {
        private CreateColumnGene _parent;

        public DropColumnGene(CreateColumnGene parent, ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
            _parent = parent;
        }

        internal override CreateColumnGene CreateColumnGene => _parent;

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites
            .Concat(new[] { _parent });

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            return _parent.DropColumnSql();
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            return _parent.CreateColumnSql();
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(DropColumnGene).Sha256Hash().Concatenate(
                _parent.Sha256Hash);
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(DropColumnGene),
                new Dictionary<string, string>
                {
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash),
                    ["Parent"] = new[] { _parent.Sha256Hash }
                });
        }

        public static DropColumnGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new DropColumnGene(
                (CreateColumnGene)genesByHashCode[memento.Prerequisites["Parent"].Single()],
                memento.Prerequisites["Prerequisites"].Select(x => genesByHashCode[x]).ToImmutableList());
        }
    }
}

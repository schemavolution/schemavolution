using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Specification.Genes
{
    class CreatePrimaryKeyGene : IndexGene
    {
        private readonly CreateTableGene _parent;
        private readonly ImmutableList<CreateColumnGene> _columns;

        public override string DatabaseName => _parent.DatabaseName;
        public override string SchemaName => _parent.SchemaName;
        public override string TableName => _parent.TableName;
        public override IEnumerable<CreateColumnGene> Columns => _columns;
        internal override CreateTableGene CreateTableGene => _parent;

        public CreatePrimaryKeyGene(CreateTableGene parent, IEnumerable<CreateColumnGene> columns, ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
            _parent = parent;
            _columns = columns.ToImmutableList();
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites
            .Concat(new[] { CreateTableGene });

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            string columnNames = string.Join(", ", _columns.Select(c => $"[{c.ColumnName}]").ToArray());
            string[] sql =
            {
                $"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]\r\n    ADD CONSTRAINT [PK_{TableName}] PRIMARY KEY CLUSTERED ({columnNames})"
            };

            return sql;
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            string[] sql =
            {
                $"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]\r\n    DROP CONSTRAINT [PK_{TableName}]"
            };

            return sql;
        }

        internal override string GenerateDefinitionSql()
        {
            string columnNames = string.Join(", ", _columns.Select(c => $"[{c.ColumnName}]").ToArray());

            return $"\r\n    CONSTRAINT [PK_{TableName}] PRIMARY KEY CLUSTERED ({columnNames})";
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CreatePrimaryKeyGene).Sha256Hash().Concatenate(
                Enumerable.Repeat(_parent.Sha256Hash, 1)
                    .Concat(_columns.Select(c => c.Sha256Hash))
                    .ToArray());
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(CreatePrimaryKeyGene),
                new Dictionary<string, string>
                {
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash),
                    ["Parent"] = new[] { _parent.Sha256Hash },
                    ["Columns"] = _columns.Select(c => c.Sha256Hash).ToArray()
                });
        }

        public static CreatePrimaryKeyGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new CreatePrimaryKeyGene(
                (CreateTableGene)genesByHashCode[memento.Prerequisites["Parent"].Single()],
                memento.Prerequisites["Columns"].Select(p => genesByHashCode[p]).OfType<CreateColumnGene>(),
                memento.Prerequisites["Prerequisites"].Select(p => genesByHashCode[p]).ToImmutableList());
        }
    }
}
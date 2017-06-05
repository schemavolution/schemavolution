using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Specification.Genes
{
    class CreateIndexGene : IndexGene
    {
        private readonly CreateTableGene _parent;
        private readonly ImmutableList<CreateColumnGene> _columns;

        public override string DatabaseName => _parent.DatabaseName;
        public override string SchemaName => _parent.SchemaName;
        public override string TableName => _parent.TableName;
        public override IEnumerable<CreateColumnGene> Columns => _columns;
        internal override CreateTableGene CreateTableGene => _parent;

        public CreateIndexGene(CreateTableGene parent, IEnumerable<CreateColumnGene> columns, ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
            _parent = parent;
            _columns = columns.ToImmutableList();
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites
            .Concat(new[] { CreateTableGene });

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            string indexTail = string.Join("_", Columns.Select(c => $"{c.ColumnName}").ToArray());
            string columnList = string.Join(", ", Columns.Select(c => $"[{c.ColumnName}]").ToArray());
            string[] sql =
            {
                $"CREATE NONCLUSTERED INDEX [IX_{TableName}_{indexTail}] ON [{DatabaseName}].[{SchemaName}].[{TableName}] ({columnList})"
            };

            return sql;
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            string indexTail = string.Join("_", Columns.Select(c => $"{c.ColumnName}").ToArray());
            string[] sql =
            {
                $"DROP INDEX [IX_{TableName}_{indexTail}] ON [{DatabaseName}].[{SchemaName}].[{TableName}]"
            };

            return sql;
        }

        internal override string GenerateDefinitionSql()
        {
            string indexTail = string.Join("_", _columns.Select(c => $"{c.ColumnName}").ToArray());
            string columnList = string.Join(", ", _columns.Select(c => $"[{c.ColumnName}]").ToArray());

            return $"\r\n    INDEX [IX_{TableName}_{indexTail}] NONCLUSTERED ({columnList})";
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CreateIndexGene).Sha256Hash().Concatenate(
                Enumerable.Repeat(_parent.Sha256Hash, 1)
                    .Concat(_columns.Select(c => c.Sha256Hash))
                    .ToArray());
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(CreateIndexGene),
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

        public static CreateIndexGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new CreateIndexGene(
                (CreateTableGene)genesByHashCode[memento.Prerequisites["Parent"].Single()],
                memento.Prerequisites["Columns"].Select(p => genesByHashCode[p]).OfType<CreateColumnGene>(),
                memento.Prerequisites["Prerequisites"].Select(p => genesByHashCode[p]).ToImmutableList());
        }
    }
}

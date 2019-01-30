using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Schemavolution.Evolve.Providers;
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

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider)
        {
            return provider.GenerateCreateIndex(DatabaseName, SchemaName, TableName, Columns.Select(c => c.ColumnName));
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider)
        {
            return provider.GenerateDropIndex(DatabaseName, SchemaName, TableName, Columns.Select(c => c.ColumnName));
        }

        internal override string GenerateDefinitionSql(IDatabaseProvider provider)
        {
            return provider.GenerateIndexDefinition(TableName, Columns.Select(c => c.ColumnName));
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Specification.Genes
{
    class CreateTableGene : Gene
    {
        private readonly UseSchemaGene _parent;
        private readonly string _tableName;

        private ImmutableList<TableDefinitionGene> _definitions =
            ImmutableList<TableDefinitionGene>.Empty;

        public string DatabaseName => _parent.DatabaseName;
        public string SchemaName => _parent.SchemaName;
        public string TableName => _tableName;

        public CreateTableGene(UseSchemaGene parent, string tableName, ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
            _parent = parent;
            _tableName = tableName;
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites
            .Concat(new[] { _parent });

        internal void AddDefinition(TableDefinitionGene childGene)
        {
            _definitions = _definitions.Add(childGene);
        }

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            string createTable;
            string head = $"CREATE TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]";
            var optimizableGenes = _definitions
                .SelectMany(m => graph.PullPrerequisitesForward(m, this, CanOptimize))
                .ToImmutableList();
            if (optimizableGenes.Any())
            {
                var definitions = optimizableGenes
                    .OfType<TableDefinitionGene>()
                    .Where(d => !d.Dropped)
                    .Select(d => d.GenerateDefinitionSql());
                createTable = $"{head}({string.Join(",", definitions)})";
                optimizableGenes = optimizableGenes.AddRange(optimizableGenes
                    .OfType<CreateColumnGene>()
                    .SelectMany(d => d.Modifications));
            }
            else
            {
                createTable = head;
            }

            string[] sql =
            {
                createTable
            };
            genesAffected.AppendAll(optimizableGenes);

            return sql;
        }

        private bool CanOptimize(Gene gene)
        {
            if (gene is TableDefinitionGene definition)
            {
                return definition.CreateTableGene == this;
            }
            else if (gene is DropColumnGene dropColumn)
            {
                return dropColumn.CreateColumnGene.CreateTableGene == this;
            }
            else if (gene is CustomSqlGene)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            string[] sql =
            {
                $"DROP TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]"
            };
            genesAffected.AppendAll(_definitions);

            return sql;
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CreateTableGene).Sha256Hash().Concatenate(
                _parent.Sha256Hash,
                _tableName.Sha256Hash());
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(CreateTableGene),
                new Dictionary<string, string>
                {
                    [nameof(TableName)] = TableName
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash),
                    ["Parent"] = new[] { _parent.Sha256Hash }
                });
        }

        public static CreateTableGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new CreateTableGene(
                (UseSchemaGene)genesByHashCode[memento.Prerequisites["Parent"].Single()],
                memento.Attributes["TableName"],
                memento.Prerequisites["Prerequisites"].Select(p => genesByHashCode[p]).ToImmutableList());
        }
    }
}
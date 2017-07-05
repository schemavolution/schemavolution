using System;
using Schemavolution.Specification.Implementation;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Collections.Immutable;

namespace Schemavolution.Specification.Genes
{
    class CreateColumnGene : TableDefinitionGene
    {
        private readonly CreateTableGene _parent;
        private readonly string _columnName;
        private readonly string _typeDescriptor;
        private readonly bool _nullable;

        private ImmutableList<ColumnModificationGene> _modifications =
            ImmutableList<ColumnModificationGene>.Empty;

        public string DatabaseName => _parent.DatabaseName;
        public string SchemaName => _parent.SchemaName;
        public string TableName => _parent.TableName;
        public string ColumnName => _columnName;
        public string TypeDescriptor => _typeDescriptor;
        public bool Nullable => _nullable;
        internal override CreateTableGene CreateTableGene => _parent;

        public CreateColumnGene(CreateTableGene parent, string columnName, string typeDescriptor, bool nullable, ImmutableList<Gene> prerequsites) :
            base(prerequsites)
        {
            _parent = parent;
            _columnName = columnName;
            _typeDescriptor = typeDescriptor;
            _nullable = nullable;
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites
            .Concat(new[] { CreateTableGene });
        public IEnumerable<ColumnModificationGene> Modifications => _modifications;

        internal void AddModification(ColumnModificationGene childGene)
        {
            _modifications = _modifications.Add(childGene);
        }

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            var optimizableGenes = _modifications
                .SelectMany(m => graph.PullPrerequisitesForward(m, this, CanOptimize))
                .ToImmutableList();

            if (optimizableGenes.Any())
            {
                genesAffected.AppendAll(optimizableGenes);

                if (optimizableGenes.OfType<DropColumnGene>().Any())
                    return new string[0];
            }

            return CreateColumnSql();
        }

        private bool CanOptimize(Gene gene)
        {
            if (gene is ColumnModificationGene modification)
            {
                return modification.CreateColumnGene == this;
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
            return DropColumnSql();
        }

        internal string[] CreateColumnSql()
        {
            string[] identityTypes = { "INT IDENTITY" };
            string[] numericTypes = { "BIGINT", "INT", "SMALLINT", "TINYINT", "MONEY", "SMALLMONEY", "DECIMAL", "FLOAT", "REAL" };
            string[] dateTypes = { "DATETIME", "SMALLDATETIME", "DATETIME2", "TIME" };
            string[] dateTimeOffsetTypes = { "DATETIMEOFFSET" };
            string[] stringTypes = { "NVARCHAR", "NCHAR", "NTEXT" };
            string[] asciiStringTypes = { "VARCHAR", "CHAR", "TEXT" };
            string[] guidTypes = { "UNIQUEIDENTIFIER" };
            string[] binaryTypes = { "BINARY", "VARBINARY" };

            string defaultExpression =
                _nullable ? null :
                identityTypes.Any(t => TypeDescriptor.StartsWith(t)) ? null :
                numericTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "0" :
                dateTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "GETUTCDATE()" :
                dateTimeOffsetTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "SYSDATETIMEOFFSET()" :
                stringTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "N''" :
                asciiStringTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "''" :
                guidTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "'00000000-0000-0000-0000-000000000000'" :
                binaryTypes.Any(t => TypeDescriptor.StartsWith(t)) ? "0x" :
                null;
            if (defaultExpression == null)
            {
                string[] sql =
                {
                    $@"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]
    ADD [{ColumnName}] {TypeDescriptor} {NullableClause}"
                };

                return sql;
            }
            else
            {
                string[] sql =
                {
                    $@"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]
    ADD [{ColumnName}] {TypeDescriptor} {NullableClause}
    CONSTRAINT [DF_{TableName}_{ColumnName}] DEFAULT ({defaultExpression})",
                    $@"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]
    DROP CONSTRAINT [DF_{TableName}_{ColumnName}]",
                };

                return sql;
            }
        }

        internal string[] DropColumnSql()
        {
            string[] sql = {
                $@"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]
    DROP COLUMN [{ColumnName}]"
            };

            return sql;
        }

        internal override bool Dropped => _modifications.OfType<DropColumnGene>().Any();

        internal override string GenerateDefinitionSql()
        {
            return $"\r\n    [{ColumnName}] {TypeDescriptor} {NullableClause}";
        }

        private string NullableClause => $"{(Nullable ? "NULL" : "NOT NULL")}";

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CreateColumnGene).Sha256Hash().Concatenate(
                _parent.Sha256Hash,
                _columnName.Sha256Hash(),
                _typeDescriptor.Sha256Hash(),
                _nullable ? "true".Sha256Hash() : "false".Sha256Hash());
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(CreateColumnGene),
                new Dictionary<string, string>
                {
                    [nameof(ColumnName)] = ColumnName,
                    [nameof(TypeDescriptor)] = TypeDescriptor,
                    [nameof(Nullable)] = Nullable ? "true" : "false"
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash),
                    ["Parent"] = new[] { _parent.Sha256Hash }
                });
        }

        public static CreateColumnGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new CreateColumnGene(
                (CreateTableGene)genesByHashCode[memento.Prerequisites["Parent"].Single()],
                memento.Attributes["ColumnName"],
                memento.Attributes["TypeDescriptor"],
                memento.Attributes["Nullable"] == "true",
                memento.Prerequisites["Prerequisites"].Select(x => genesByHashCode[x]).ToImmutableList());
        }
    }
}

using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Schemavolution.Specification
{
    public class TableSpecification : Specification
    {
        private readonly CreateTableGene _gene;

        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal TableSpecification(CreateTableGene gene, EvolutionHistoryBuilder evolutionHistoryBuilder) :
            base(evolutionHistoryBuilder)
        {
            _gene = gene;
        }

        private TableSpecification(CreateTableGene gene, EvolutionHistoryBuilder evolutionHistoryBuilder, ImmutableList<Gene> prerequisites) :
            base(evolutionHistoryBuilder, prerequisites)
        {
            _gene = gene;
        }

        public TableSpecification After(params Specification[] specifications)
        {
            return new TableSpecification(_gene, EvolutionHistoryBuilder,
                Prerequisites.AddRange(specifications.SelectMany(x => x.Genes)));
        }

        public ColumnSpecification CreateIdentityColumn(string columnName)
        {
            return CreateColumn(columnName, "INT IDENTITY (1,1)", false);
        }

        public ColumnSpecification CreateBigIntColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "BIGINT", nullable);
        }

        public ColumnSpecification CreateIntColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "INT", nullable);
        }

        public ColumnSpecification CreateSmallIntColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "SMALLINT", nullable);
        }

        public ColumnSpecification CreateTinyIntColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "TINYINT", nullable);
        }

        public ColumnSpecification CreateBitColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "BIT", nullable);
        }

        public ColumnSpecification CreateMoneyColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "MONEY", nullable);
        }

        public ColumnSpecification CreateSmallMoneyColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "SMALLMONEY", nullable);
        }

        public ColumnSpecification CreateDecimalColumn(string columnName, int precision = 18, int scale = 0, bool nullable = false)
        {
            return CreateColumn(columnName, $"DECIMAL({precision},{scale})", nullable);
        }

        public ColumnSpecification CreateFloatColumn(string columnName, int mantissa = 57, bool nullable = false)
        {
            return CreateColumn(columnName, $"FLOAT({mantissa})", nullable);
        }

        public ColumnSpecification CreateRealColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "REAL", nullable);
        }

        public ColumnSpecification CreateDateColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "DATE", nullable);
        }

        public ColumnSpecification CreateDateTimeColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "DATETIME", nullable);
        }

        public ColumnSpecification CreateSmallDateTimeColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "SMALLDATETIME", nullable);
        }

        public ColumnSpecification CreateDateTime2Column(string columnName, int fractionalSeconds = 7, bool nullable = false)
        {
            return CreateColumn(columnName, $"DATETIME2({fractionalSeconds})", nullable);
        }

        public ColumnSpecification CreateTimeColumn(string columnName, int fractionalSeconds = 7, bool nullable = false)
        {
            return CreateColumn(columnName, $"TIME({fractionalSeconds})", nullable);
        }

        public ColumnSpecification CreateDateTimeOffsetColumn(string columnName, int fractionalSeconds = 7, bool nullable = false)
        {
            return CreateColumn(columnName, $"DATETIMEOFFSET({fractionalSeconds})", nullable);
        }

        public ColumnSpecification CreateStringColumn(string columnName, int length, bool nullable = false)
        {
            return CreateColumn(columnName, $"NVARCHAR({length})", nullable);
        }

        public ColumnSpecification CreateFixedStringColumn(string columnName, int length, bool nullable = false)
        {
            return CreateColumn(columnName, $"NCHAR({length})", nullable);
        }

        public ColumnSpecification CreateTextColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "NTEXT", nullable);
        }

        public ColumnSpecification CreateStringMaxColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "NVARCHAR(MAX)", nullable);
        }

        public ColumnSpecification CreateAsciiStringColumn(string columnName, int length, bool nullable = false)
        {
            return CreateColumn(columnName, $"VARCHAR({length})", nullable);
        }

        public ColumnSpecification CreateFixedAsciiStringColumn(string columnName, int length, bool nullable = false)
        {
            return CreateColumn(columnName, $"CHAR({length})", nullable);
        }

        public ColumnSpecification CreateAsciiTextColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "TEXT", nullable);
        }

        public ColumnSpecification CreateAsciiStringMaxColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "VARCHAR(MAX)", nullable);
        }

        public ColumnSpecification CreateGuidColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "UNIQUEIDENTIFIER", nullable);
        }

        public ColumnSpecification CreateBinaryColumn(string columnName, int length, bool nullable = false)
        {
            return CreateColumn(columnName, $"VARBINARY({length})", nullable);
        }

        public ColumnSpecification CreateBinaryMaxColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, $"VARBINARY(MAX)", nullable);
        }

        public ColumnSpecification CreateFixedBinaryColumn(string columnName, int length, bool nullable = false)
        {
            return CreateColumn(columnName, $"BINARY({length})", nullable);
        }

        public ColumnSpecification CreateJsonColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "JSON", nullable);
        }

        public ColumnSpecification CreateJsonBinaryColumn(string columnName, bool nullable = false)
        {
            return CreateColumn(columnName, "JSONB", nullable);
        }

        private ColumnSpecification CreateColumn(string columnName, string typeDescriptor, bool nullable)
        {
            var childGene = new CreateColumnGene(
                _gene,
                columnName,
                typeDescriptor,
                nullable,
                Prerequisites);
            EvolutionHistoryBuilder.Append(childGene);
            childGene.AddToParent();
            return new ColumnSpecification(childGene, EvolutionHistoryBuilder);
        }

        public PrimaryKeySpecification CreatePrimaryKey(params ColumnSpecification[] columns)
        {
            var childGene = new CreatePrimaryKeyGene(
                _gene,
                columns.Select(c => c.Gene),
                Prerequisites);
            EvolutionHistoryBuilder.Append(childGene);
            childGene.AddToParent();
            return new PrimaryKeySpecification(childGene, EvolutionHistoryBuilder);
        }

        public UniqueIndexSpecification CreateUniqueIndex(params ColumnSpecification[] columns)
        {
            var childGene = new CreateUniqueIndexGene(
                _gene,
                columns.Select(c => c.Gene),
                Prerequisites);
            EvolutionHistoryBuilder.Append(childGene);
            childGene.AddToParent();
            return new UniqueIndexSpecification(childGene, EvolutionHistoryBuilder);
        }

        public IndexSpecification CreateIndex(params ColumnSpecification[] columns)
        {
            var childGene = new CreateIndexGene(
                _gene,
                columns.Select(c => c.Gene),
                Prerequisites);
            EvolutionHistoryBuilder.Append(childGene);
            childGene.AddToParent();
            return new IndexSpecification(childGene, EvolutionHistoryBuilder);
        }
    }
}
using Newtonsoft.Json;
using Schemavolution.Specification.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Schemavolution.Evolve.Providers
{
    public class SqlServerProvider : IDatabaseProvider
    {
        public string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes)
        {
            string[] values = genes.Select(gene => GenerateGeneValue(gene)).ToArray();
            var insert = $@"INSERT INTO [{databaseName}].[dbo].[__EvolutionHistory]
    ([Type], [HashCode], [Attributes])
    VALUES{String.Join(",", values)}";
            return insert;
        }

        private string GenerateGeneValue(GeneMemento gene)
        {
            string attributes = JsonConvert.SerializeObject(gene.Attributes);
            string hex = $"0x{gene.HashCode.ToString("X64")}";
            return $@"
    ('{gene.Type}', {hex}, '{attributes.Replace("'", "''")}')";
        }

        public string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes)
        {
            var joins =
                from gene in genes
                from role in gene.Prerequisites
                from prerequisite in role.Value
                select new { GeneHashCode = gene.HashCode, Role = role.Key, PrerequisiteHashCode = prerequisite };
            string[] values = joins.Select((join, i) => GeneratePrerequisiteSelect(databaseName, join.GeneHashCode, join.Role, i + 1, join.PrerequisiteHashCode)).ToArray();
            string sql = $@"INSERT INTO [{databaseName}].[dbo].[__EvolutionHistoryPrerequisite]
    ([GeneId], [Role], [Ordinal], [PrerequisiteGeneId]){string.Join(@"
UNION ALL", values)}";
            return sql;
        }

        string GeneratePrerequisiteSelect(string databaseName, BigInteger geneHashCode, string role, int ordinal, BigInteger prerequisiteHashCode)
        {
            return $@"
SELECT m.GeneId, '{role}', {ordinal}, p.GeneId
FROM [{databaseName}].[dbo].[__EvolutionHistory] m,
     [{databaseName}].[dbo].[__EvolutionHistory] p
WHERE m.HashCode = 0x{geneHashCode.ToString("X64")} AND p.HashCode = 0x{prerequisiteHashCode.ToString("X64")}";
        }

        public string GenerateCreateTable(string databaseName, string schemaName, string tableName, IEnumerable<string> definitions)
        {
            string head = $"CREATE TABLE [{databaseName}].[{schemaName}].[{tableName}]";
            if (definitions.Any())
                return $"{head}({string.Join(",", definitions)})";
            else
                return head;
        }

        public string GenerateDropTable(string databaseName, string schemaName, string tableName)
        {
            return $"DROP TABLE [{databaseName}].[{schemaName}].[{tableName}]";
        }

        public string[] GenerateCreateColumn(string databaseName, string schemaName, string tableName, string columnName, string typeDescriptor, bool nullable)
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
                nullable ? null :
                identityTypes.Any(t => typeDescriptor.StartsWith(t)) ? null :
                numericTypes.Any(t => typeDescriptor.StartsWith(t)) ? "0" :
                dateTypes.Any(t => typeDescriptor.StartsWith(t)) ? "GETUTCDATE()" :
                dateTimeOffsetTypes.Any(t => typeDescriptor.StartsWith(t)) ? "SYSDATETIMEOFFSET()" :
                stringTypes.Any(t => typeDescriptor.StartsWith(t)) ? "N''" :
                asciiStringTypes.Any(t => typeDescriptor.StartsWith(t)) ? "''" :
                guidTypes.Any(t => typeDescriptor.StartsWith(t)) ? "'00000000-0000-0000-0000-000000000000'" :
                binaryTypes.Any(t => typeDescriptor.StartsWith(t)) ? "0x" :
                null;
            if (defaultExpression == null)
            {
                string[] sql =
                {
                    $@"ALTER TABLE [{databaseName}].[{schemaName}].[{tableName}]
    ADD [{columnName}] {typeDescriptor} {(nullable ? "NULL" : "NOT NULL")}"
                };

                return sql;
            }
            else
            {
                string[] sql =
                {
                    $@"ALTER TABLE [{databaseName}].[{schemaName}].[{tableName}]
    ADD [{columnName}] {typeDescriptor} {(nullable ? "NULL" : "NOT NULL")}
    CONSTRAINT [DF_{tableName}_{columnName}] DEFAULT ({defaultExpression})",
                    $@"ALTER TABLE [{databaseName}].[{schemaName}].[{tableName}]
    DROP CONSTRAINT [DF_{tableName}_{columnName}]",
                };

                return sql;
            }
        }

        public string[] GenerateDropColumn(string databaseName, string schemaName, string tableName, string columnName)
        {
            return new string[] {
                $@"ALTER TABLE [{databaseName}].[{schemaName}].[{tableName}]
    DROP COLUMN [{columnName}]"
            };
        }

        public string GenerateColumnDefinition(string columnName, string typeDescriptor, bool nullable)
        {
            return $"\r\n    [{columnName}] {typeDescriptor} {(nullable ? "NULL" : "NOT NULL")}";
        }

        public string[] GenerateCreatePrimaryKey(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            string quotedColumnNames = string.Join(", ", columnNames.Select(c => $"[{c}]").ToArray());
            string[] sql =
            {
                $"ALTER TABLE [{databaseName}].[{schemaName}].[{tableName}]\r\n    ADD CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({quotedColumnNames})"
            };

            return sql;
        }

        public string[] GenerateDropPrimaryKey(string databaseName, string schemaName, string tableName)
        {
            return new string[] {
                $"ALTER TABLE [{databaseName}].[{schemaName}].[{tableName}]\r\n    DROP CONSTRAINT [PK_{tableName}]"
            };
        }

        public string GeneratePrimaryKeyDefinition(string tableName, IEnumerable<string> columnNames)
        {
            string quotedColumnNames = string.Join(", ", columnNames.Select(c => $"[{c}]").ToArray());

            return $"\r\n    CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ({quotedColumnNames})";
        }

        public string[] GenerateCreateIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string columnList = string.Join(", ", columnNames.Select(c => $"[{c}]").ToArray());
            string[] sql =
            {
                $"CREATE NONCLUSTERED INDEX [IX_{tableName}_{indexTail}] ON [{databaseName}].[{schemaName}].[{tableName}] ({columnList})"
            };

            return sql;
        }

        public string[] GenerateDropIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string[] sql =
            {
                $"DROP INDEX [IX_{tableName}_{indexTail}] ON [{databaseName}].[{schemaName}].[{tableName}]"
            };

            return sql;
        }

        public string GenerateIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string columnList = string.Join(", ", columnNames.Select(c => $"[{c}]").ToArray());

            return $"\r\n    INDEX [IX_{tableName}_{indexTail}] NONCLUSTERED ({columnList})";
        }

        public string[] GenerateCreateUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string columnList = string.Join(", ", columnNames.Select(c => $"[{c}]").ToArray());
            string[] sql =
            {
                $"CREATE UNIQUE NONCLUSTERED INDEX [UX_{tableName}_{indexTail}] ON [{databaseName}].[{schemaName}].[{tableName}] ({columnList})"
            };

            return sql;
        }

        public string[] GenerateDropUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string[] sql =
            {
                $"DROP INDEX [UX_{tableName}_{indexTail}] ON [{databaseName}].[{schemaName}].[{tableName}]"
            };

            return sql;
        }

        public string GenerateUniqueIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string columnList = string.Join(", ", columnNames.Select(c => $"[{c}]").ToArray());

            return $"\r\n    INDEX [UX_{tableName}_{indexTail}] UNIQUE NONCLUSTERED ({columnList})";
        }
    }
}

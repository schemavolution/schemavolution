using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Evolve.Providers
{
    class PostgreSqlProvider : IDatabaseProvider
    {
        public string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes)
        {
            string[] values = genes.Select(gene => GenerateGeneValue(gene)).ToArray();
            var insert = $@"INSERT INTO ""public"".""__evolution_history""
    (""type"", ""hash_code"", ""attributes"")
    VALUES{String.Join(",", values)};";
            return insert;
        }

        private string GenerateGeneValue(GeneMemento gene)
        {
            string attributes = JsonConvert.SerializeObject(gene.Attributes);
            string hex = $@"'\x{gene.HashCode.ToString("X64")}'::bytea";
            return $@"
    ('{gene.Type}', {hex}, '{attributes.Replace("'", "''")}'::jsonb)";
        }

        public string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes)
        {
            var joins =
                from gene in genes
                from role in gene.Prerequisites
                from prerequisite in role.Value
                select new { GeneHashCode = gene.HashCode, Role = role.Key, PrerequisiteHashCode = prerequisite };
            string[] values = joins.Select((join, i) => GeneratePrerequisiteSelect(databaseName, join.GeneHashCode, join.Role, i + 1, join.PrerequisiteHashCode)).ToArray();
            string sql = $@"INSERT INTO ""public"".""__evolution_history_prerequisite""
    (""gene_id"", ""role"", ""ordinal"", ""prerequisite_gene_id""){string.Join(@"
UNION ALL", values)};";
            return sql;
        }

        private string GeneratePrerequisiteSelect(string databaseName, BigInteger geneHashCode, string role, int ordinal, BigInteger prerequisiteHashCode)
        {
            return $@"
SELECT m.gene_id, '{role}', {ordinal}, p.gene_id
FROM ""public"".""__evolution_history"" m,
     ""public"".""__evolution_history"" p
WHERE m.hash_code = '\x{geneHashCode.ToString("X64")}'::bytea AND p.hash_code = '\x{prerequisiteHashCode.ToString("X64")}'::bytea";
        }

        public string GenerateCreateTable(string databaseName, string schemaName, string tableName, IEnumerable<string> definitions)
        {
            string head = $@"CREATE TABLE ""{schemaName}"".""{tableName}""";
            if (definitions.Any())
                return $"{head}({string.Join(",", definitions)})";
            else
                return head;
        }

        public string GenerateDropTable(string databaseName, string schemaName, string tableName)
        {
            throw new System.NotImplementedException("GenerateDropTable");
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
                dateTypes.Any(t => typeDescriptor.StartsWith(t)) ? "now() at time zone 'utc'" :
                dateTimeOffsetTypes.Any(t => typeDescriptor.StartsWith(t)) ? "now() at time zone 'utc'" :
                stringTypes.Any(t => typeDescriptor.StartsWith(t)) ? "''" :
                asciiStringTypes.Any(t => typeDescriptor.StartsWith(t)) ? "''" :
                guidTypes.Any(t => typeDescriptor.StartsWith(t)) ? "'00000000-0000-0000-0000-000000000000'" :
                binaryTypes.Any(t => typeDescriptor.StartsWith(t)) ? "\x00::bytea" :
                null;
            if (defaultExpression == null)
            {
                string[] sql =
                {
                    $@"ALTER TABLE ""{schemaName}"".""{tableName}""
    ADD ""{columnName}"" {TranslateTypeDescriptor(typeDescriptor)} {(nullable ? "NULL" : "NOT NULL")};"
                };

                return sql;
            }
            else
            {
                string[] sql =
                {
                    $@"ALTER TABLE ""{schemaName}"".""{tableName}""
    ADD ""{columnName}"" {TranslateTypeDescriptor(typeDescriptor)} {(nullable ? "NULL" : "NOT NULL")}
    DEFAULT ({defaultExpression});",
                    $@"ALTER TABLE ""{schemaName}"".""{tableName}""
    ALTER COLUMN ""{columnName}""
    DROP DEFAULT;",
                };

                return sql;
            }
        }

        public string[] GenerateDropColumn(string databaseName, string schemaName, string tableName, string columnName)
        {
            throw new System.NotImplementedException("GenerateDropColumn");
        }

        public string GenerateColumnDefinition(string columnName, string typeDescriptor, bool nullable)
        {
            return $"\r\n    \"{columnName}\" {TranslateTypeDescriptor(typeDescriptor)} {(nullable ? "NULL" : "NOT NULL")}";
        }

        private object TranslateTypeDescriptor(string typeDescriptor)
        {
            if (typeDescriptor.StartsWith("NVARCHAR"))
            {
                return new Regex("NVARCHAR").Replace(typeDescriptor, "character varying");
            }
            throw new NotImplementedException();
        }

        public string[] GenerateCreatePrimaryKey(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateCreatePrimaryKey");
        }

        public string[] GenerateDropPrimaryKey(string databaseName, string schemaName, string tableName)
        {
            throw new System.NotImplementedException("GenerateDropPrimaryKey");
        }

        public string GeneratePrimaryKeyDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GeneratePrimaryKeyDefinition");
        }

        public string[] GenerateCreateIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateCreateIndex");
        }

        public string[] GenerateDropIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateDropIndex");
        }

        public string GenerateIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateIndexDefinition");
        }

        public string[] GenerateCreateUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            string indexTail = string.Join("_", columnNames.ToArray());
            string columnList = string.Join(", ", columnNames.Select(c => $"\"{c}\"").ToArray());
            string[] sql =
            {
                $@"CREATE UNIQUE INDEX ""{tableName}_{indexTail}_ux"" ON ""{schemaName}"".""{tableName}"" ({columnList})"
            };

            return sql;
        }

        public string[] GenerateDropUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateDropUniqueIndex");
        }

        public string GenerateUniqueIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateUniqueIndexDefinition");
        }
    }
}
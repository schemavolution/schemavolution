﻿using System.Collections.Generic;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Evolve.Providers
{
    class PostgreSqlProvider : IDatabaseProvider
    {
        public string[] GenerateInitialization(string databaseName, string fileName)
        {
            return new string[]{
                $@"CREATE DATABASE ""{databaseName}"";",
                $@"\connect {databaseName}",
                $@"CREATE TABLE ""__evolution_history"" (
                    ""gene_id"" serial PRIMARY KEY,
                    ""type"" varchar(50) NOT NULL,
                    ""hash_code"" bit varying(256) NOT NULL UNIQUE,
                    ""attributes"" jsonb
                );",
                @"CREATE TABLE ""__evolution_history_prerequisite"" (
                    ""gene_id"" int NOT NULL REFERENCES ""__evolution_history"",
                    ""role"" varchar(50) NOT NULL,
                    ""ordinal"" int NOT NULL,
                    ""prerequisite_gene_id"" int NOT NULL REFERENCES ""__evolution_history""
                );",
                @"CREATE INDEX ""__evolution_history_prerequisite_gene_id_idx""
                    ON ""__evolution_history_prerequisite""(""gene_id"");",
                @"CREATE INDEX ""__evolution_history_prerequisite_prerequisite_gene_id_idx""
                    ON ""__evolution_history_prerequisite""(""prerequisite_gene_id"");"
            };
        }

        public string GenerateColumnDefinition(string columnName, string typeDescriptor, bool nullable)
        {
            throw new System.NotImplementedException("GenerateColumnDefinition");
        }

        public string[] GenerateCreateColumn(string databaseName, string schemaName, string tableName, string columnName, string typeDescriptor, bool nullable)
        {
            throw new System.NotImplementedException("GenerateCreateColumn");
        }

        public string[] GenerateCreateIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateCreateIndex");
        }

        public string[] GenerateCreatePrimaryKey(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateCreatePrimaryKey");
        }

        public string GenerateCreateTable(string databaseName, string schemaName, string tableName, IEnumerable<string> definitions)
        {
            throw new System.NotImplementedException("GenerateCreateTable");
        }

        public string[] GenerateCreateUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateCreateUniqueIndex");
        }

        public string[] GenerateDropColumn(string databaseName, string schemaName, string tableName, string columnName)
        {
            throw new System.NotImplementedException("GenerateDropColumn");
        }

        public string[] GenerateDropIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateDropIndex");
        }

        public string[] GenerateDropPrimaryKey(string databaseName, string schemaName, string tableName)
        {
            throw new System.NotImplementedException("GenerateDropPrimaryKey");
        }

        public string GenerateDropTable(string databaseName, string schemaName, string tableName)
        {
            throw new System.NotImplementedException("GenerateDropTable");
        }

        public string[] GenerateDropUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateDropUniqueIndex");
        }

        public string GenerateIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateIndexDefinition");
        }

        public string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes)
        {
            throw new System.NotImplementedException("GenerateInsertStatement");
        }

        public string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes)
        {
            throw new System.NotImplementedException("GeneratePrerequisiteInsertStatements");
        }

        public string GeneratePrimaryKeyDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GeneratePrimaryKeyDefinition");
        }

        public string GenerateUniqueIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException("GenerateUniqueIndexDefinition");
        }
    }
}
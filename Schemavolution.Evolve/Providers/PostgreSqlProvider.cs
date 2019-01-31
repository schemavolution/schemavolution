using System.Collections.Generic;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Evolve.Providers
{
    class PostgreSqlProvider : IDatabaseProvider
    {
        public string GenerateColumnDefinition(string columnName, string typeDescriptor, bool nullable)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateCreateColumn(string databaseName, string schemaName, string tableName, string columnName, string typeDescriptor, bool nullable)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateCreateIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateCreatePrimaryKey(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateCreateTable(string databaseName, string schemaName, string tableName, IEnumerable<string> definitions)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateCreateUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateDropColumn(string databaseName, string schemaName, string tableName, string columnName)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateDropIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateDropPrimaryKey(string databaseName, string schemaName, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateDropTable(string databaseName, string schemaName, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateDropUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string[] GenerateInitialization(string databaseName, string fileName)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes)
        {
            throw new System.NotImplementedException();
        }

        public string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes)
        {
            throw new System.NotImplementedException();
        }

        public string GeneratePrimaryKeyDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }

        public string GenerateUniqueIndexDefinition(string tableName, IEnumerable<string> columnNames)
        {
            throw new System.NotImplementedException();
        }
    }
}
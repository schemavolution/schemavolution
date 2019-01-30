using System.Collections.Generic;
using System.Collections.Immutable;
using Schemavolution.Specification.Genes;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Evolve.Providers
{
    public interface IDatabaseProvider
    {
        string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes);
        string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes);
        string GenerateCreateTable(string databaseName, string schemaName, string tableName, IEnumerable<string> definitions);
        string GenerateDropTable(string databaseName, string schemaName, string tableName);
        string[] GenerateCreateColumn(string databaseName, string schemaName, string tableName, string columnName, string typeDescriptor, bool nullable);
        string[] GenerateDropColumn(string databaseName, string schemaName, string tableName, string columnName);
        string GenerateColumnDefinition(string columnName, string typeDescriptor, bool nullable);
        string[] GenerateCreatePrimaryKey(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames);
        string[] GenerateDropPrimaryKey(string databaseName, string schemaName, string tableName);
        string GeneratePrimaryKeyDefinition(string tableName, IEnumerable<string> columnNames);
        string[] GenerateCreateIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames);
        string[] GenerateDropIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames);
        string GenerateIndexDefinition(string tableName, IEnumerable<string> columnNames);
        string[] GenerateCreateUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames);
        string[] GenerateDropUniqueIndex(string databaseName, string schemaName, string tableName, IEnumerable<string> columnNames);
        string GenerateUniqueIndexDefinition(string tableName, IEnumerable<string> columnNames);
    }
}
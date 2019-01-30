using System.Collections.Generic;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Evolve.Providers
{
    public interface IDatabaseProvider
    {
        string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes);
        string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes);
        string GenerateCreateTable(string databaseName, string schemaName, string tableName, IEnumerable<string> definitions);
        string[] GenerateCreateColumn(string databaseName, string schemaName, string tableName, string columnName, string typeDescriptor, bool nullable);
    }
}
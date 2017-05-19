﻿using System;
using MergableMigrations.Specification.Implementation;

namespace MergableMigrations.Specification
{
    public class TableSpecification
    {
        private readonly string _databaseName;
        private readonly string _schemaName;
        private readonly string _tableName;
        private readonly MigrationHistoryBuilder _migrationHistoryBuilder;
        private readonly CreateTableMigration _migration;

        internal TableSpecification(string databaseName, string schemaName, string tableName, MigrationHistoryBuilder migrationHistoryBuilder, CreateTableMigration migration)
        {
            _databaseName = databaseName;
            _schemaName = schemaName;
            _tableName = tableName;
            _migrationHistoryBuilder = migrationHistoryBuilder;
            _migration = migration;
        }

        public ColumnSpecification CreateIntColumn(string columnName, bool nullable = false)
        {
            var childMigration = new CreateColumnMigration(
                _databaseName, _schemaName, _tableName,
                columnName, $"INT {(nullable ? "NULL" : "NOT NULL")}",
                _migrationHistoryBuilder);
            _migration.AddColumn(childMigration);
            _migrationHistoryBuilder.Append(childMigration);
            return new ColumnSpecification();
        }

        public ColumnSpecification CreateStringColumn(string name, int length, bool nullable = false)
        {
            return new ColumnSpecification();
        }

        public PrimaryKeySpecification CreatePrimaryKey(params ColumnSpecification[] columns)
        {
            return new PrimaryKeySpecification();
        }

        public UniqueIndexSpecification CreateUniqueIndex(params ColumnSpecification[] columns)
        {
            throw new NotImplementedException();
        }

        public IndexSpecification CreateIndex(params ColumnSpecification[] columns)
        {
            return new IndexSpecification();
        }
    }
}
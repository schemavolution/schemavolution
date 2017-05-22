﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace MergableMigrations.Specification.Implementation
{
    class CreateDatabaseMigration : Migration
    {
        private readonly string _databaseName;

        public string DatabaseName => _databaseName;

        public CreateDatabaseMigration(string databaseName)
        {
            _databaseName = databaseName;
        }

        public override string[] GenerateSql(MigrationHistoryBuilder migrationsAffected)
        {
            string[] sql = { };
            return sql;
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CreateDatabaseMigration).Sha256Hash().Concatenate(
                _databaseName.Sha256Hash());
        }

        internal override MigrationMemento GetMemento()
        {
            return new MigrationMemento(
                nameof(CreateDatabaseMigration),
                new Dictionary<string, string>
                {
                    [nameof(DatabaseName)] = DatabaseName
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>> { });
        }
    }
}

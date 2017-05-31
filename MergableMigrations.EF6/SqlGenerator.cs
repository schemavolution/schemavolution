﻿using MergableMigrations.Specification;
using MergableMigrations.Specification.Implementation;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System;
using System.Collections.Generic;

namespace MergableMigrations.EF6
{
    public class SqlGenerator
    {
        private readonly IMigrations _migrations;
        private readonly MigrationHistory _migrationHistory;

        public SqlGenerator(IMigrations migrations, MigrationHistory migrationHistory)
        {
            _migrations = migrations;
            _migrationHistory = migrationHistory;
        }

        public string[] Generate(string databaseName)
        {
            var newMigrations = GetMigrationHistory(databaseName);
            var ahead = _migrationHistory.Subtract(newMigrations);
            if (ahead.Any)
                throw new InvalidOperationException(
                    "The target database is ahead of the desired migration. You can force a rollback, which may destroy data.");
            var difference = newMigrations.Subtract(_migrationHistory);

            var sql = ImmutableList<string>.Empty;

            while (difference.Any)
            {
                var migrationsAffected = new MigrationHistoryBuilder();
                migrationsAffected.Append(difference.Head);
                string[] result = difference.Head.GenerateSql(migrationsAffected);
                sql = sql.AddRange(result);
                var mementos = migrationsAffected.MigrationHistory.GetMementos().ToList();
                sql = sql.Add(GenerateInsertStatement(databaseName, mementos));
                if (mementos.SelectMany(m => m.Prerequisites).SelectMany(p => p.Value).Any())
                    sql = sql.Add(GeneratePrerequisiteInsertStatements(databaseName, mementos));
                difference = difference.Subtract(migrationsAffected.MigrationHistory);
            }

            return sql.ToArray();
        }

        public string[] GenerateRollbackSql(string databaseName)
        {
            var newMigrations = GetMigrationHistory(databaseName);
            var ahead = _migrationHistory.Subtract(newMigrations);

            var sql = ImmutableList<string>.Empty;

            while (ahead.Any)
            {
                var migrationsAffected = new MigrationHistoryBuilder();
                migrationsAffected.Append(ahead.Head);
                string[] rollbackSql = ahead.Head.GenerateRollbackSql(migrationsAffected);
                var mementos = migrationsAffected.MigrationHistory.GetMementos().ToList();
                string[] deleteStatements = GenerateDeleteStatements(databaseName, mementos);
                sql = sql.InsertRange(0, deleteStatements);
                sql = sql.InsertRange(0, rollbackSql);
                ahead = ahead.Subtract(migrationsAffected.MigrationHistory);
            }

            return sql.ToArray();
        }

        private MigrationHistory GetMigrationHistory(string databaseName)
        {
            var databaseSpecification = new DatabaseSpecification(databaseName);
            _migrations.AddMigrations(databaseSpecification);
            return databaseSpecification.MigrationHistory;
        }

        private string GenerateInsertStatement(string databaseName, IEnumerable<MigrationMemento> migrations)
        {
            string[] values = migrations.Select(migration => GenerateMigrationValue(migration)).ToArray();
            var insert = $@"INSERT INTO [{databaseName}].[dbo].[__MergableMigrationHistory]
    ([Type], [HashCode], [Attributes])
    VALUES{String.Join(",", values)}";
            return insert;
        }

        private string GenerateMigrationValue(MigrationMemento migration)
        {
            string attributes = JsonConvert.SerializeObject(migration.Attributes);
            string hex = $"0x{migration.HashCode.ToString("X")}";
            return $@"
    ('{migration.Type}', {hex}, '{attributes}')";
        }

        private string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<MigrationMemento> migrations)
        {
            var joins =
                from migration in migrations
                from role in migration.Prerequisites
                from prerequisite in role.Value
                select new { MigrationHashCode = migration.HashCode, Role = role.Key, PrerequisiteHashCode = prerequisite };
            string[] values = joins.Select(join => GeneratePrerequisiteSelect(databaseName, join.MigrationHashCode, join.Role, join.PrerequisiteHashCode)).ToArray();
            string sql = $@"INSERT INTO [{databaseName}].[dbo].[__MergableMigrationHistoryPrerequisite]
    ([MigrationId], [Role], [PrerequisiteMigrationId]){string.Join(@"
UNION ALL", values)}";
            return sql;
        }

        string GeneratePrerequisiteSelect(string databaseName, BigInteger migrationHashCode, string role, BigInteger prerequisiteHashCode)
        {
            return $@"
SELECT m.MigrationId, '{role}', p.MigrationId
FROM [{databaseName}].[dbo].[__MergableMigrationHistory] m,
     [{databaseName}].[dbo].[__MergableMigrationHistory] p
WHERE m.HashCode = 0x{migrationHashCode.ToString("X")} AND p.HashCode = 0x{prerequisiteHashCode.ToString("X")}";
        }

        private string[] GenerateDeleteStatements(string databaseName, IEnumerable<MigrationMemento> migrations)
        {
            var hashCodes = string.Join(", ", migrations.Select(m => $"0x{m.HashCode.ToString("X")}"));
            string[] sql =
            {
                $@"DELETE p
FROM [{databaseName}].[dbo].[__MergableMigrationHistory] m
JOIN [{databaseName}].[dbo].[__MergableMigrationHistoryPrerequisite] p
  ON p.MigrationId = m.MigrationId
WHERE m.HashCode IN ({hashCodes})",
                $@"DELETE FROM [{databaseName}].[dbo].[__MergableMigrationHistory]
WHERE HashCode IN ({hashCodes})"
            };
            return sql;
        }
    }
}
﻿using MergableMigrations.Specification.Implementation;
using System.Collections.Generic;
using System.Linq;

namespace MergableMigrations.Specification
{
    public class ForeignKeySpecification : Specification
    {
        internal override IEnumerable<Migration> Migrations => Enumerable.Empty<Migration>();

        internal ForeignKeySpecification(MigrationHistoryBuilder migrationHistoryBuilder) :
            base(migrationHistoryBuilder)
        {
        }
    }
}
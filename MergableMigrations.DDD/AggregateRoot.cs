﻿using MergableMigrations.Specification;

namespace MergableMigrations.DDD
{
    public class AggregateRoot
    {
        public PrimaryKeySpecification PrimaryKey { get; }

        public AggregateRoot(PrimaryKeySpecification primaryKey)
        {
            PrimaryKey = primaryKey;
        }
    }
}

using Schemavolution.Specification.Genes;
using System;
using System.Collections.Immutable;
using System.Numerics;

namespace Schemavolution.Specification.Implementation
{
    static class GeneLoader
    {
        public static Gene Load(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            switch (memento.Type)
            {
                case nameof(UseSchemaGene):
                    return UseSchemaGene.FromMemento(memento, genesByHashCode);
                case nameof(CreateTableGene):
                    return CreateTableGene.FromMemento(memento, genesByHashCode);
                case nameof(CreateColumnGene):
                    return CreateColumnGene.FromMemento(memento, genesByHashCode);
                case nameof(CreatePrimaryKeyGene):
                    return CreatePrimaryKeyGene.FromMemento(memento, genesByHashCode);
                case nameof(CreateUniqueIndexGene):
                    return CreateUniqueIndexGene.FromMemento(memento, genesByHashCode);
                case nameof(CreateIndexGene):
                    return CreateIndexGene.FromMemento(memento, genesByHashCode);
                case nameof(CreateForeignKeyGene):
                    return CreateForeignKeyGene.FromMemento(memento, genesByHashCode);
                case nameof(CustomSqlGene):
                    return CustomSqlGene.FromMemento(memento, genesByHashCode);
                default:
                    throw new ArgumentException($"Unknown type {memento.Type}");
            }
        }
    }
}
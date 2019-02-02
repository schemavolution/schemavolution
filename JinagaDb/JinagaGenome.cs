using Schemavolution.Specification;
using System;

namespace JinagaDb
{
    public class JinagaGenome : IGenome
    {
        public RdbmsIdentifier Rdbms => RdbmsIdentifier.PostgreSQL;

        public void AddGenes(DatabaseSpecification databaseSpecification)
        {
            var p = databaseSpecification.UseSchema("public");

            DefineEdge(p);
            DefineFact(p);
        }

        private static void DefineEdge(SchemaSpecification p)
        {
            var edge = p.CreateTable("edge");
            var successorType = edge.CreateStringColumn("successor_type", 50);
            var successorHash = edge.CreateStringColumn("successor_hash", 100);
            var predecessorType = edge.CreateStringColumn("predecessor_type", 50);
            var predecessorHash = edge.CreateStringColumn("predecessor_hash", 100);
            var role = edge.CreateStringColumn("role", 20);

            // Most unique first, for fastest uniqueness check on insert.
            edge.CreateUniqueIndex(
                successorHash,
                predecessorHash,
                role,
                successorType,
                predecessorType);
            // Covering index based on successor, favoring most likely members of WHERE clause.
            edge.CreateIndex(
                successorHash,
                role,
                successorType,
                predecessorHash,
                predecessorType);
            // Covering index based on predecessor, favoring most likely members of WHERE clause.
            edge.CreateIndex(
                predecessorHash,
                role,
                predecessorType,
                successorHash,
                successorType);
        }

        private static void DefineFact(SchemaSpecification p)
        {
            var fact = p.CreateTable("fact");

            var type = fact.CreateStringColumn("type", 50);
            var hash = fact.CreateStringColumn("hash", 100);
            var fields = fact.CreateJsonBinaryColumn("fields");
            var predecessors = fact.CreateJsonBinaryColumn("predecessors");

            fact.CreateUniqueIndex(hash, type);
        }
    }
}

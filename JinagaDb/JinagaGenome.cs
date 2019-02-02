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

            var edge = p.CreateTable("edge");
            var successorType = edge.CreateStringColumn("successor_type", 50);
            var successorHash = edge.CreateStringColumn("successor_hash", 100);
            var predecessorType = edge.CreateStringColumn("predecessor_type", 50);
            var predecessorHash = edge.CreateStringColumn("predecessor_hash", 100);
            var role = edge.CreateStringColumn("role", 20);

            edge.CreateUniqueIndex(
                successorHash,
                predecessorHash,
                role,
                successorType,
                predecessorType);
        }
    }
}

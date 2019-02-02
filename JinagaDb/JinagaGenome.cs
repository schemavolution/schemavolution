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
            edge.CreateStringColumn("successor_type", 50);
            edge.CreateStringColumn("successor_hash", 100);
        }
    }
}

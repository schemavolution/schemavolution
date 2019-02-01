using Schemavolution.Specification;
using System;

namespace JinagaDb
{
    public class JinagaGenome : IGenome
    {
        public RdbmsIdentifier Rdbms => RdbmsIdentifier.PostgreSQL;

        public void AddGenes(DatabaseSpecification databaseSpecification)
        {
            throw new NotImplementedException();
        }
    }
}

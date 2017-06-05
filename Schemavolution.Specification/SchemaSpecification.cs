using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System;
using System.Collections.Generic;

namespace Schemavolution.Specification
{
    public class SchemaSpecification : Specification
    {
        private readonly UseSchemaGene _gene;

        internal override IEnumerable<Gene> Genes => new[] { _gene };

        internal SchemaSpecification(UseSchemaGene gene, EvolutionHistoryBuilder evolutionHistoryBuilder) :
            base(evolutionHistoryBuilder)
        {
            _gene = gene;
        }

        public TableSpecification CreateTable(string tableName)
        {
            var gene = new CreateTableGene(_gene, tableName, Prerequisites);
            EvolutionHistoryBuilder.Append(gene);
            gene.AddToParent();
            return new TableSpecification(gene, EvolutionHistoryBuilder);
        }
    }
}
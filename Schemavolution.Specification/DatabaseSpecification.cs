using System;
using Schemavolution.Specification.Implementation;
using Schemavolution.Specification.Genes;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Schemavolution.Specification
{
    public class DatabaseSpecification : Specification
    {
        private readonly string _databaseName;

        public EvolutionHistory EvolutionHistory => EvolutionHistoryBuilder.EvolutionHistory;
        internal override IEnumerable<Gene> Genes => Enumerable.Empty<Gene>();

        public DatabaseSpecification(string databaseName) :
            base(new EvolutionHistoryBuilder())
        {
            _databaseName = databaseName;
        }

        private DatabaseSpecification(string databaseName, EvolutionHistoryBuilder evolutionHistoryBuilder, ImmutableList<Gene> prerequisites) :
            base(evolutionHistoryBuilder, prerequisites)
        {
            _databaseName = databaseName;
        }

        public DatabaseSpecification After(params Specification[] specifications)
        {
            return new DatabaseSpecification(_databaseName, EvolutionHistoryBuilder,
                Prerequisites.AddRange(specifications.SelectMany(x => x.Genes)));
        }

        public SchemaSpecification UseSchema(string schemaName)
        {
            var gene = new UseSchemaGene(_databaseName, schemaName, Prerequisites);
            EvolutionHistoryBuilder.Append(gene);
            gene.AddToParent();
            return new SchemaSpecification(gene, EvolutionHistoryBuilder);
        }

        public CustomSqlSpecification Execute(string up, string down = null)
        {
            var gene = new CustomSqlGene(_databaseName, up, down, Prerequisites);
            EvolutionHistoryBuilder.Append(gene);
            gene.AddToParent();
            return new CustomSqlSpecification(gene, EvolutionHistoryBuilder);
        }
    }
}
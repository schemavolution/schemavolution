using System;
using System.Collections.Generic;
using System.Numerics;
using Schemavolution.Specification.Implementation;
using System.Linq;
using System.Collections.Immutable;
using Schemavolution.Evolve.Providers;

namespace Schemavolution.Specification.Genes
{
    class UseSchemaGene : Gene
    {
        private readonly string _databaseName;
        private readonly string _schemaName;

        public string DatabaseName => _databaseName;
        public string SchemaName => _schemaName;

        public UseSchemaGene(string databaseName, string schemaName, ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
            _databaseName = databaseName;
            _schemaName = schemaName;
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites;

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider)
        {
            string[] sql = { };
            return sql;
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider)
        {
            throw new NotImplementedException();
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(UseSchemaGene).Sha256Hash().Concatenate(
                _schemaName.Sha256Hash());
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(UseSchemaGene),
                new Dictionary<string, string>
                {
                    [nameof(DatabaseName)] = DatabaseName,
                    [nameof(SchemaName)] = SchemaName
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash)
                });
        }

        public static UseSchemaGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new UseSchemaGene(
                memento.Attributes["DatabaseName"],
                memento.Attributes["SchemaName"],
                memento.Prerequisites["Prerequisites"].Select(p => genesByHashCode[p]).ToImmutableList());
        }
    }
}
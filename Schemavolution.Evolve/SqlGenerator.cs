using Schemavolution.Evolve.Generator;
using Schemavolution.Evolve.Providers;
using Schemavolution.Specification;
using Schemavolution.Specification.Implementation;
using System;
using System.Linq;

namespace Schemavolution.Evolve
{
    public class SqlGenerator
    {
        private readonly IGenome _genome;
        private readonly EvolutionHistory _evolutionHistory;
        private readonly IDatabaseProvider _provider;

        public SqlGenerator(IGenome genome, EvolutionHistory evolutionHistory, IDatabaseProvider provider)
        {
            _genome = genome;
            _evolutionHistory = evolutionHistory;
            _provider = provider;
        }

        public string[] Generate(string databaseName)
        {
            var newGenes = GetEvolutionHistory(databaseName);
            var ahead = _evolutionHistory.Subtract(newGenes);
            if (ahead.Any)
                throw new InvalidOperationException(
                    "The target database is ahead of the desired genome. Execute \"Evolve-Database -Force\" or call \"DevolveDatabase()\" on the DatabaseEvolver to roll back, which may destroy data.");
            var difference = newGenes.Subtract(_evolutionHistory);

            var generator = new ForwardGenerator(databaseName, difference, _provider);

            while (generator.Any)
            {
                generator.AddGene(generator.Head);
            }

            return generator.Sql.ToArray();
        }

        public string[] GenerateRollbackSql(string databaseName)
        {
            var newGenes = GetEvolutionHistory(databaseName);
            var ahead = _evolutionHistory.Subtract(newGenes);

            var generator = new RollbackGenerator(databaseName, ahead, _provider);

            while (generator.Any)
            {
                generator.AddGene(generator.Head);
            }

            return generator.Sql.ToArray();
        }

        private EvolutionHistory GetEvolutionHistory(string databaseName)
        {
            var databaseSpecification = new DatabaseSpecification(databaseName);
            _genome.AddGenes(databaseSpecification);
            return databaseSpecification.EvolutionHistory;
        }
    }
}
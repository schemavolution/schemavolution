using Schemavolution.Evolve.Providers;
using Schemavolution.Specification.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Schemavolution.Evolve.Generator
{
    class ForwardGenerator : IGraphVisitor
    {
        private readonly string _databaseName;
        private readonly SqlServerProvider _provider;

        private EvolutionDelta _difference;
        private ImmutableList<string> _sql = ImmutableList<string>.Empty;
        private ImmutableStack<Gene> _working = ImmutableStack<Gene>.Empty;

        public ForwardGenerator(string databaseName, EvolutionDelta difference, SqlServerProvider provider)
        {
            _databaseName = databaseName;
            _provider = provider;
            _difference = difference;
        }

        public bool Any => _difference.Any;
        public Gene Head => _difference.Head;
        public ImmutableList<string> Sql => _sql;

        public bool AddGene(Gene gene)
        {
            if (_working.Contains(gene))
                return false;

            foreach (var prerequisite in gene.AllPrerequisites
                .Where(p => _difference.Contains(p)))
            {
                if (!AddGene(prerequisite))
                    return false;
            }

            _working = _working.Push(gene);

            var genesAffected = new EvolutionHistoryBuilder();
            genesAffected.Append(gene);
            string[] result = gene.GenerateSql(genesAffected, this, _provider);
            _sql = _sql.AddRange(result);
            var mementos = genesAffected.EvolutionHistory.GetMementos().ToList();
            _sql = _sql.Add(_provider.GenerateInsertStatement(_databaseName, mementos));
            if (mementos.SelectMany(m => m.Prerequisites).SelectMany(p => p.Value).Any())
                _sql = _sql.Add(_provider.GeneratePrerequisiteInsertStatements(_databaseName, mementos));
            _difference = _difference.Subtract(genesAffected.EvolutionHistory);

            _working = _working.Pop();

            return true;
        }

        public ImmutableList<Gene> PullPrerequisitesForward(Gene gene, Gene origin, Func<Gene, bool> canOptimize)
        {
            ImmutableList<Gene> optimizableGenes =
                ImmutableList<Gene>.Empty;

            foreach (var prerequisite in gene.AllPrerequisites
                .Where(p => p != origin)
                .Where(p => _difference.Contains(p)))
            {
                if (Follows(prerequisite, origin))
                {
                    if (canOptimize(prerequisite))
                    {
                        optimizableGenes = optimizableGenes.Add(prerequisite);
                    }
                    else
                    {
                        return ImmutableList<Gene>.Empty;
                    }
                }
                else
                {
                    if (!AddGene(prerequisite))
                    {
                        return ImmutableList<Gene>.Empty;
                    }
                }
            }

            optimizableGenes = optimizableGenes.Add(gene);
            return optimizableGenes;
        }

        private bool Follows(Gene gene, Gene origin)
        {
            return gene.AllPrerequisites
                .Any(p => p == origin || Follows(p, origin));
        }
    }
}

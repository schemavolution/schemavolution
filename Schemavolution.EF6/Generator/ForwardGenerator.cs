using Schemavolution.Specification.Implementation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace Schemavolution.EF6.Generator
{
    class ForwardGenerator : IGraphVisitor
    {
        private readonly string _databaseName;

        private EvolutionDelta _difference;
        private ImmutableList<string> _sql = ImmutableList<string>.Empty;
        private ImmutableStack<Gene> _working = ImmutableStack<Gene>.Empty;

        public ForwardGenerator(string databaseName, EvolutionDelta difference)
        {
            _databaseName = databaseName;
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
            string[] result = gene.GenerateSql(genesAffected, this);
            _sql = _sql.AddRange(result);
            var mementos = genesAffected.EvolutionHistory.GetMementos().ToList();
            _sql = _sql.Add(GenerateInsertStatement(_databaseName, mementos));
            if (mementos.SelectMany(m => m.Prerequisites).SelectMany(p => p.Value).Any())
                _sql = _sql.Add(GeneratePrerequisiteInsertStatements(_databaseName, mementos));
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

        private bool Follows(Gene migration, Gene origin)
        {
            return migration.AllPrerequisites
                .Any(p => p == origin || Follows(p, origin));
        }

        private string GenerateInsertStatement(string databaseName, IEnumerable<GeneMemento> genes)
        {
            string[] values = genes.Select(gene => GenerateGeneValue(gene)).ToArray();
            var insert = $@"INSERT INTO [{databaseName}].[dbo].[__MergableMigrationHistory]
    ([Type], [HashCode], [Attributes])
    VALUES{String.Join(",", values)}";
            return insert;
        }

        private string GenerateGeneValue(GeneMemento gene)
        {
            string attributes = JsonConvert.SerializeObject(gene.Attributes);
            string hex = $"0x{gene.HashCode.ToString("X")}";
            return $@"
    ('{gene.Type}', {hex}, '{attributes.Replace("'", "''")}')";
        }

        private string GeneratePrerequisiteInsertStatements(string databaseName, IEnumerable<GeneMemento> genes)
        {
            var joins =
                from gene in genes
                from role in gene.Prerequisites
                from prerequisite in role.Value
                select new { GeneHashCode = gene.HashCode, Role = role.Key, PrerequisiteHashCode = prerequisite };
            string[] values = joins.Select(join => GeneratePrerequisiteSelect(databaseName, join.GeneHashCode, join.Role, join.PrerequisiteHashCode)).ToArray();
            string sql = $@"INSERT INTO [{databaseName}].[dbo].[__MergableMigrationHistoryPrerequisite]
    ([MigrationId], [Role], [PrerequisiteMigrationId]){string.Join(@"
UNION ALL", values)}";
            return sql;
        }

        string GeneratePrerequisiteSelect(string databaseName, BigInteger geneHashCode, string role, BigInteger prerequisiteHashCode)
        {
            return $@"
SELECT m.MigrationId, '{role}', p.MigrationId
FROM [{databaseName}].[dbo].[__MergableMigrationHistory] m,
     [{databaseName}].[dbo].[__MergableMigrationHistory] p
WHERE m.HashCode = 0x{geneHashCode.ToString("X")} AND p.HashCode = 0x{prerequisiteHashCode.ToString("X")}";
        }
    }
}

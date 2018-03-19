using Schemavolution.Specification.Implementation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;

namespace Schemavolution.Sql.Generator
{
    class RollbackGenerator : IGraphVisitor
    {
        private readonly string _databaseName;

        private ImmutableList<string> _sql = ImmutableList<string>.Empty;
        private EvolutionDelta _ahead;

        public RollbackGenerator(string databaseName, EvolutionDelta ahead)
        {
            _databaseName = databaseName;
            _ahead = ahead;
        }

        public bool Any => _ahead.Any;
        public Gene Head => _ahead.Head;
        public ImmutableList<string> Sql => _sql;

        public void AddGene(Gene gene)
        {
            var genesAffected = new EvolutionHistoryBuilder();
            genesAffected.Append(gene);
            string[] rollbackSql = gene.GenerateRollbackSql(genesAffected, this);
            var mementos = genesAffected.EvolutionHistory.GetMementos().ToList();
            string[] deleteStatements = GenerateDeleteStatements(_databaseName, mementos);
            _sql = _sql.InsertRange(0, deleteStatements);
            _sql = _sql.InsertRange(0, rollbackSql);
            _ahead = _ahead.Subtract(genesAffected.EvolutionHistory);
        }

        public ImmutableList<Gene> PullPrerequisitesForward(Gene gene, Gene origin, Func<Gene, bool> canOptimize)
        {
            throw new NotImplementedException();
        }

        private string[] GenerateDeleteStatements(string databaseName, IEnumerable<GeneMemento> genes)
        {
            var hashCodes = string.Join(", ", genes.Select(m => $"0x{m.HashCode.ToString("X64")}"));
            string[] sql =
            {
                $@"DELETE p
FROM [{databaseName}].[dbo].[__EvolutionHistory] m
JOIN [{databaseName}].[dbo].[__EvolutionHistoryPrerequisite] p
  ON p.GeneId = m.GeneId
WHERE m.HashCode IN ({hashCodes})",
                $@"DELETE FROM [{databaseName}].[dbo].[__EvolutionHistory]
WHERE HashCode IN ({hashCodes})"
            };
            return sql;
        }
    }
}
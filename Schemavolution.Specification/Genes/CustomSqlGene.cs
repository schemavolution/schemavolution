using Schemavolution.Specification.Implementation;
using System.Collections.Immutable;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace Schemavolution.Specification.Genes
{
    class CustomSqlGene : Gene
    {
        private readonly string _databaseName;
        private readonly string _up;
        private readonly string _down;

        public string DatabaseName => _databaseName;
        public string Up => _up;
        public string Down => _down;

        public CustomSqlGene(string databaseName, string up, string down, ImmutableList<Gene> prerequisites) :
            base(prerequisites)
        {
            _databaseName = databaseName;
            _up = up;
            _down = down;
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites;

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            return new string[] { $"USE {DatabaseName}", _up };
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph)
        {
            return
                _down != null ? new string[] { $"USE {DatabaseName}", _down } :
                new string[] { };
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CustomSqlGene).Sha256Hash().Concatenate(
                _up.Sha256Hash(),
                _down.Sha256Hash());
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(CustomSqlGene),
                new Dictionary<string, string>
                {
                    [nameof(DatabaseName)] = DatabaseName,
                    [nameof(Up)] = Up,
                    [nameof(Down)] = Down
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash)
                });
        }

        public static CustomSqlGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new CustomSqlGene(
                memento.Attributes["DatabaseName"],
                memento.Attributes["Up"],
                memento.Attributes["Down"],
                memento.Prerequisites["Prerequisites"].Select(p => genesByHashCode[p]).ToImmutableList());
        }
    }
}
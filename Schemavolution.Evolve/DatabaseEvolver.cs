using Newtonsoft.Json;
using Schemavolution.Evolve.Executor;
using Schemavolution.Evolve.Loader;
using Schemavolution.Evolve.Providers;
using Schemavolution.Specification;
using Schemavolution.Specification.Implementation;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Schemavolution.Evolve
{
    public class DatabaseEvolver
    {
        private readonly string _databaseName;
        private readonly string _fileName;
        private readonly IGenome _genome;
        private readonly IDatabaseProvider _provider;
        private readonly IDatabaseExecutor _executor;

        public static DatabaseEvolver ForGenome(string databaseName, string filename, string masterConnectionString, IGenome genome)
        {
            if (genome.Rdbms == RdbmsIdentifier.MSSqlServer)
            {
                return new DatabaseEvolver(databaseName, filename, genome, new SqlServerProvider(), new SqlServerExecutor(masterConnectionString, databaseName));
            }
            else if (genome.Rdbms == RdbmsIdentifier.PostgreSQL)
            {
                return new DatabaseEvolver(databaseName, null, genome, new PostgreSqlProvider(), new PostgreSqlExecutor(masterConnectionString, databaseName));
            }
            else
            {
                throw new System.ArgumentException($"Unknown RDBMS {genome.Rdbms}");
            }
        }

        private DatabaseEvolver(string databaseName, string fileName, IGenome genome, IDatabaseProvider provider, IDatabaseExecutor executor)
        {
            _databaseName = databaseName;
            _fileName = fileName;
            _genome = genome;
            _provider = provider;
            _executor = executor;
        }

        public bool EvolveDatabase()
        {
            if (!_executor.DatabaseExists())
            {
                _executor.CreateDatabase(_fileName);
            }
            var evolutionHistory = LoadEvolutionHistory();

            var generator = new SqlGenerator(_genome, evolutionHistory, _provider);

            var sql = generator.Generate(_databaseName);
            _executor.ExecuteSqlCommands(sql);

            return sql.Any();
        }

        public bool DevolveDatabase()
        {
            var evolutionHistory = LoadEvolutionHistory();
            var generator = new SqlGenerator(_genome, evolutionHistory, _provider);
            var sql = generator.GenerateRollbackSql(_databaseName);

            _executor.ExecuteSqlCommands(sql);

            return sql.Any();
        }

        public void DestroyDatabase()
        {
            _executor.DestroyDatabase();
        }

        private EvolutionHistory LoadEvolutionHistory()
        {
            _executor.UpgradeDatabase();

            var rows = _executor.LoadEvolutionHistory();

            return EvolutionHistory.LoadMementos(LoadMementos(rows));
        }

        private static IEnumerable<GeneMemento> LoadMementos(
            IEnumerable<EvolutionHistoryRow> rows)
        {
            var enumerator = new LookaheadEnumerator<EvolutionHistoryRow>(rows.GetEnumerator());
            enumerator.MoveNext();
            if (enumerator.More)
            {
                do
                {
                    yield return LoadMemento(enumerator);
                } while (enumerator.More);
            }
        }

        private static GeneMemento LoadMemento(LookaheadEnumerator<EvolutionHistoryRow> enumerator)
        {
            var type = enumerator.Current.Type;
            var hashCode = enumerator.Current.HashCode;
            var attributes = enumerator.Current.Attributes;
            var roles = LoadRoles(hashCode, enumerator);

            var geneAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(attributes);
            var memento = new GeneMemento(
                type,
                geneAttributes,
                hashCode,
                roles);
            return memento;
        }

        private static IDictionary<string, IEnumerable<BigInteger>> LoadRoles(BigInteger hashCode, LookaheadEnumerator<EvolutionHistoryRow> enumerator)
        {
            var result = new Dictionary<string, IEnumerable<BigInteger>>();
            do
            {
                string role = enumerator.Current.Role;
                if (role != null)
                {
                    var prerequisites = LoadPrerequisites(hashCode, role, enumerator).ToList();
                    result[role] = prerequisites;
                }
                else
                {
                    enumerator.MoveNext();
                }
            } while (enumerator.More && enumerator.Current.HashCode == hashCode);

            return result;
        }

        private static IEnumerable<BigInteger> LoadPrerequisites(BigInteger hashCode, string role, LookaheadEnumerator<EvolutionHistoryRow> enumerator)
        {
            do
            {
                yield return enumerator.Current.PrerequisiteHashCode;
                enumerator.MoveNext();
            } while (enumerator.More && enumerator.Current.HashCode == hashCode && enumerator.Current.Role == role);
        }
    }
}

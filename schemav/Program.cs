using Schemavolution.Evolve;
using Schemavolution.Specification;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace schemav
{
    class Program
    {
        const int MSSqlServer = 0x01;
        const int PostgreSQL = 0x02;

        static void Main(string[] args)
        {
            var flags = args.Where(a => a.StartsWith("-")).ToArray();
            var parameters = args.Where(a => !a.StartsWith("-")).ToArray();
            var force = flags.Contains("-f");
            var provider =
                (flags.Contains("-pg") ? PostgreSQL : 0) |
                (flags.Contains("-ms") ? MSSqlServer : 0);

            if (parameters.Length == 3 &&
                (provider == PostgreSQL || provider == MSSqlServer))
            {
                string assemblyPath = parameters[0];
                string databaseName = parameters[1];
                string masterConnectionString = parameters[2];
                EvolveDatabase(assemblyPath, force, g =>
                    provider == MSSqlServer ? DatabaseEvolver
                        .ForSqlServer(databaseName, masterConnectionString, g) :
                    provider == PostgreSQL ? DatabaseEvolver
                        .ForPostgreSQL(databaseName, masterConnectionString, g) :
                    null);
            }
            else
            {
                Console.WriteLine("schemav");
                Console.WriteLine("  Schemavolution command line interface");
                Console.WriteLine();
                Console.WriteLine("  schemav <assembly path> <database name> <master connection string>");
                Console.WriteLine("");
                Console.WriteLine("  Flags:");
                Console.WriteLine("    -f  Force (optional) - Devolve the database.");
                Console.WriteLine("                           Required after genes have been deleted.");
                Console.WriteLine("    -pg PostgreSQL       - Exactly one provider must be selected.");
                Console.WriteLine("    -ms MS SQL Server");
            }
        }

        private static void EvolveDatabase(string assemblyPath, bool force, Func<IGenome, DatabaseEvolver> createEvolver)
        {
            string assemblyDirectory = Directory.GetParent(assemblyPath).FullName;
            var resolver = new AssemblyResolver(AppDomain.CurrentDomain, assemblyDirectory);

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var types = assembly.GetExportedTypes();
                var genomeTypes = types.Where(t => typeof(IGenome).IsAssignableFrom(t));
                if (genomeTypes.Count() > 1)
                    throw new ArgumentException("The assembly contains more than one genome type.");
                if (genomeTypes.Count() == 0)
                    throw new ArgumentException("The assembly does not contain a genome type.");

                var genomeTypeName = genomeTypes.Single().FullName;
                var genome = (IGenome)assembly.CreateInstance(genomeTypeName);

                var evolver = createEvolver(genome);
                if (force)
                {
                    if (evolver.DevolveDatabase())
                        Console.WriteLine("Database successfully devolved.");
                    else
                        Console.WriteLine("No devolution necessary.");
                }
                if (evolver.EvolveDatabase())
                    Console.WriteLine("Database successfully evolved.");
                else
                    Console.WriteLine("No evolution necessary.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                resolver.Detach();
            }
        }
    }
}

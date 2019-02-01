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
        static void Main(string[] args)
        {
            var flags = args.Where(a => a.StartsWith("-")).ToArray();
            var parameters = args.Where(a => !a.StartsWith("-")).ToArray();
            var force = flags.Contains("-f");

            if (parameters.Length == 3)
            {
                string assemblyPath = parameters[0];
                string databaseName = parameters[1];
                string masterConnectionString = parameters[2];
                EvolveDatabase(assemblyPath, force, databaseName, masterConnectionString);
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
            }
        }

        private static void EvolveDatabase(string assemblyPath, bool force, string databaseName, string masterConnectionString)
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

                var evolver = DatabaseEvolver.ForGenome(databaseName, null, masterConnectionString, genome);
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

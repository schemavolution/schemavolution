using Schemavolution.Sql;
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
            if (args.Length == 3)
            {
                EvolveDatabase(args[0], args[1], args[2], false);
            }
            else if (args.Length == 4)
            {
                EvolveDatabase(args[0], args[1], args[2], args[3] == "-f");
            }
            else
            {
                Console.WriteLine("schemav");
                Console.WriteLine("  Schemavolution command line interface");
            }
        }

        private static void EvolveDatabase(string assemblyPath, string databaseName, string masterConnectionString, bool force)
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

                var evolver = new DatabaseEvolver(
                    databaseName,
                    masterConnectionString,
                    genome);
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

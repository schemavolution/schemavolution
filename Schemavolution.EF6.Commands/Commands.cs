using Schemavolution.Specification;
using System;
using System.Linq;
using System.Reflection;

namespace Schemavolution.EF6.Commands
{
    public static class Commands
    {
        public static void EvolveDatabase(string assemblyPath, string databaseName, string masterConnectionString, bool force)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var types = assembly.GetExportedTypes();
            var genomeTypes = types.Where(t => typeof(IGenome).IsAssignableFrom(t));
            if (genomeTypes.Count() > 1)
                throw new ArgumentException("The assembly contains more than one genome type.");
            if (genomeTypes.Count() == 0)
                throw new ArgumentException("The assembly does not contain a genome type.");

            var genome = (IGenome)Activator.CreateInstance(genomeTypes.Single());

            var evolver = new DatabaseEvolver(
                databaseName,
                masterConnectionString,
                genome);
            if (force)
                evolver.DevolveDatabase();
            evolver.EvolveDatabase();
        }
    }
}

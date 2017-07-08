using System;
using System.IO;
using System.Reflection;

namespace schemav
{
    public class AssemblyResolver : MarshalByRefObject
    {
        private readonly AppDomain _appDomain;
        private readonly string _targetDirectory;

        public AssemblyResolver(AppDomain appDomain, string targetDirectory)
        {
            _appDomain = appDomain;
            _targetDirectory = targetDirectory;
            _appDomain.AssemblyResolve += AppDomain_AssemblyResolve;
        }

        private Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            var assemblyFileName = Path.Combine(_targetDirectory, assemblyName + ".dll");
            if (File.Exists(assemblyFileName))
                return Assembly.LoadFrom(assemblyFileName);

            return null;
        }

        public void Detach()
        {
            _appDomain.AssemblyResolve -= AppDomain_AssemblyResolve;
        }
    }
}

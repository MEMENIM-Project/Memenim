using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Memenim.Script
{
    public class ScriptLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _dependencyResolver;

        public ScriptLoadContext(string name, string path)
            : base(name, true)
        {
            _dependencyResolver = new AssemblyDependencyResolver(path);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = _dependencyResolver
                .ResolveAssemblyToPath(assemblyName);

            if (assemblyPath == null)
                return null;

            return LoadFromAssemblyPath(assemblyPath);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _dependencyResolver.
                ResolveUnmanagedDllToPath(unmanagedDllName);

            if (libraryPath == null)
                return IntPtr.Zero;

            return LoadUnmanagedDllFromPath(libraryPath);
        }
    }
}

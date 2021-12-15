using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Memenim.Scripting
{
    public class ScriptLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _dependencyResolver;



        public ScriptLoadContext(
            string name, string path)
            : base(name, true)
        {
            _dependencyResolver = new AssemblyDependencyResolver(path);
        }



        protected override Assembly Load(
            AssemblyName assemblyName)
        {
            var assemblyPath = _dependencyResolver
                .ResolveAssemblyToPath(assemblyName);

            if (assemblyPath == null)
                return null;

            return LoadFromAssemblyPath(assemblyPath);
        }

        protected override IntPtr LoadUnmanagedDll(
            string unmanagedDllPath)
        {
            var assemblyPath = _dependencyResolver.
                ResolveUnmanagedDllToPath(unmanagedDllPath);

            if (assemblyPath == null)
                return IntPtr.Zero;

            return LoadUnmanagedDllFromPath(assemblyPath);
        }
    }
}

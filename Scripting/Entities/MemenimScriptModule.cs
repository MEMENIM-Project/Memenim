using System;
using System.IO;
using System.Reflection;
using Memenim.Scripting.Core;
using RIS;

namespace Memenim.Scripting.Entities
{
    public class MemenimScriptModule : IDisposable
    {
        public string DirectoryPath { get; private set; }
        public string DirectoryName { get; private set; }

        public ScriptLoadContext Context { get; private set; }
        public string ContextName { get; private set; }
        public Assembly AssemblyFile { get; private set; }
        public string AssemblyPath { get; private set; }
        public string AssemblyName { get; private set; }
        public string AssemblyExtension { get; private set; }

        public MemenimScriptBase Script { get; private set; }
        public string Name { get; private set; }



        public MemenimScriptModule(
            string directoryPath)
        {
            Load(directoryPath);
        }



        private void Load(
            string directoryPath)
        {
            var directorySeparators = new[]
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            };

            directoryPath = directoryPath
                .TrimEnd(directorySeparators);
            directoryPath = !Path.IsPathRooted(directoryPath)
                ? Path.GetFullPath(directoryPath)
                : directoryPath;

            if (!File.Exists(directoryPath))
            {
                var exception = new DirectoryNotFoundException(
                    $"Directory['{directoryPath}'] not found");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }

            DirectoryPath = directoryPath;
            DirectoryName = directoryPath
                .Substring(directoryPath
                    .LastIndexOfAny(directorySeparators));

            var assemblyFiles = Directory
                .GetFiles(directoryPath, "*.dll");

            if (assemblyFiles.Length == 0)
            {
                var exception = new ArgumentException(
                    $"Directory['{directoryPath}'] does not contain any assembly files",
                    nameof(directoryPath));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }

            var depsFiles = Directory.GetFiles(
                directoryPath, "*.deps.json");

            if (depsFiles.Length > 1 && !string.IsNullOrEmpty(depsFiles[0]))
            {
                var mainAssemblyFileName =
                    depsFiles[0][..^".deps.json".Length];
                var mainAssemblyFilePath = Path.Combine(
                    directoryPath, $"{mainAssemblyFileName}.dll");

                ContextName = $"script:[{DirectoryPath}]";
                Context = new ScriptLoadContext(
                    ContextName, mainAssemblyFilePath);

                Assembly mainAssembly;

                try
                {
                    var assemblyName = new AssemblyName(
                        mainAssemblyFileName);

                    mainAssembly = Context
                        .LoadFromAssemblyName(assemblyName);
                }
                catch (Exception)
                {
                    var exception = new ArgumentException(
                        $"Unable to load script main assembly['{mainAssemblyFilePath}']",
                        nameof(directoryPath));
                    Events.OnError(new RErrorEventArgs(exception,
                        exception.Message));
                    throw exception;
                }

                foreach (var assemblyType in mainAssembly.GetTypes())
                {
                    if (!typeof(MemenimScriptBase).IsAssignableFrom(assemblyType))
                        continue;

                    AssemblyFile = mainAssembly;
                    AssemblyPath = mainAssemblyFilePath;
                    AssemblyName = Path.GetFileNameWithoutExtension(
                        mainAssemblyFileName);
                    AssemblyExtension = Path.GetExtension(
                        mainAssemblyFileName);

                    Script = (MemenimScriptBase)Activator.CreateInstance(
                        assemblyType, true);

                    break;
                }

                if (AssemblyFile == null)
                {
                    Unload();

                    var exception = new ArgumentException(
                        $"Script main assembly['{mainAssemblyFilePath}'] does not contain an implementation of MemenimScriptBase class",
                        nameof(directoryPath));
                    Events.OnError(new RErrorEventArgs(exception,
                        exception.Message));
                    throw exception;
                }
            }
            else
            {
                var exception = new ArgumentException(
                    $"Directory['{directoryPath}'] does not contain script main assembly file (with implementation of MemenimScriptBase class)",
                    nameof(directoryPath));
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }

            Name = DirectoryName;

            ScriptManager.OnLoaded(this, new ScriptLoadedEventArgs(
                this));
        }



        public void Unload(
            Exception sourceException = null)
        {
            if (Context.IsCollectible)
                Context?.Unload();

            ScriptManager.OnUnloaded(this,
                new ScriptUnloadedEventArgs(
                    sourceException, this));
        }



        public void Dispose()
        {
            Unload();
        }
    }
}

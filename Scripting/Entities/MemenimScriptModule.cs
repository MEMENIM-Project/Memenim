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

        public MemenimScriptModule(string directoryPath)
        {
            Load(directoryPath);
        }

        private void Load(string directoryPath)
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

            ContextName = $"script:[{DirectoryPath}]";
            Context = new ScriptLoadContext(ContextName, DirectoryPath);

            var depsFiles = Directory.GetFiles(directoryPath, "*.deps.json");

            if (depsFiles.Length > 1 && !string.IsNullOrEmpty(depsFiles[0]))
            {
                var mainAssemblyFileName = depsFiles[0].Substring(0, depsFiles[0].Length - ".deps.json".Length);
                var mainAssemblyFile = Path.Combine(directoryPath, $"{mainAssemblyFileName}.dll");

                foreach (var assemblyFile in assemblyFiles)
                {
                    if (assemblyFile != mainAssemblyFile)
                        continue;

                    Assembly assembly;

                    try
                    {
                        var assemblyName = new AssemblyName(
                            Path.GetFileNameWithoutExtension(assemblyFile));
                        assembly = Context
                            .LoadFromAssemblyName(assemblyName);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    foreach (var assemblyType in assembly.GetTypes())
                    {
                        if (!typeof(MemenimScriptBase).IsAssignableFrom(assemblyType))
                            continue;

                        AssemblyFile = assembly;
                        AssemblyPath = assemblyFile;
                        AssemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
                        AssemblyExtension = Path.GetExtension(assemblyFile);

                        Script = (MemenimScriptBase)Activator.CreateInstance(
                            assemblyType, true);

                        break;
                    }

                    break;
                }
            }

            if (AssemblyFile == null)
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

        public void Unload(Exception sourceException = null)
        {
            if (!Context.IsCollectible)
                return;

            Context?.Unload();

            ScriptManager.OnUnloaded(this, new ScriptUnloadedEventArgs(
                sourceException, this));
        }

        public void Dispose()
        {
            Unload();
        }
    }
}

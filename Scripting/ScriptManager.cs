using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Memenim.Scripting.Entities;
using RIS;
using Environment = RIS.Environment;

namespace Memenim.Scripting
{
    public static class ScriptManager
    {
        public static event EventHandler<ScriptInformationEventArgs> Information;
        public static event EventHandler<ScriptWarningEventArgs> Warning;
        public static event EventHandler<ScriptErrorEventArgs> Error;

        public static event EventHandler<ScriptLoadedEventArgs> Loaded;
        public static event EventHandler<ScriptUnloadedEventArgs> Unloaded;



        public static readonly string CustomScriptsDirectoryPath;



        public static ReadOnlyDictionary<string, MemenimScriptModule> Scripts { get; private set; }



        static ScriptManager()
        {
            var baseProcessDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseProcessDirectory) || baseProcessDirectory == "Unknown")
                baseProcessDirectory = string.Empty;

            CustomScriptsDirectoryPath = Path.Combine(
                baseProcessDirectory, "scripts", "MEMENIM", "custom");

            if (!Directory.Exists(CustomScriptsDirectoryPath))
                Directory.CreateDirectory(CustomScriptsDirectoryPath);
        }



        public static void OnInformation(
            ScriptInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender,
            ScriptInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(
            ScriptWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender,
            ScriptWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(
            ScriptErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender,
            ScriptErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }


        public static void OnLoaded(
            ScriptLoadedEventArgs e)
        {
            OnLoaded(null, e);
        }
        public static void OnLoaded(object sender,
            ScriptLoadedEventArgs e)
        {
            Loaded?.Invoke(sender, e);
        }

        public static void OnUnloaded(
            ScriptUnloadedEventArgs e)
        {
            OnUnloaded(null, e);
        }
        public static void OnUnloaded(object sender,
            ScriptUnloadedEventArgs e)
        {
            Unloaded?.Invoke(sender, e);
        }



        private static Dictionary<string, string> GetScriptsPaths(
            string directoryBasePath)
        {
            var scriptsPaths = new Dictionary<string, string>(10);
            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return scriptsPaths;
            }

            if (!Directory.Exists(directory))
                return scriptsPaths;

            foreach (var directoryPath in Directory.EnumerateDirectories(directory))
            {
                try
                {
                    if (string.IsNullOrEmpty(directoryPath))
                        continue;

                    var directoryPathFixed = directoryPath
                        .TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar);
                    directoryPathFixed = $"{directoryPathFixed}{Path.DirectorySeparatorChar}";

                    var directoryName = Path
                        .GetDirectoryName(directoryPathFixed);

                    if (string.IsNullOrEmpty(directoryName))
                        continue;

                    scriptsPaths[directoryName] = directoryPath;
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
            }

            return scriptsPaths;
        }

        private static Dictionary<string, MemenimScriptModule> GetScripts()
        {
            var scriptsPaths = new Dictionary<string, string>(10);

            if (!string.IsNullOrEmpty(CustomScriptsDirectoryPath))
            {
                foreach (var (key, value) in GetScriptsPaths(
                             CustomScriptsDirectoryPath))
                {
                    scriptsPaths[key] = value;
                }
            }

            var scripts = new Dictionary<string, MemenimScriptModule>(10);

            foreach (var (key, value) in scriptsPaths)
            {
                try
                {
                    var script = new MemenimScriptModule(
                        value);

                    scripts[key] = script;
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
            }

            return scripts;
        }

        private static void LoadScripts()
        {
            var scripts = GetScripts();

            Scripts = new ReadOnlyDictionary<string, MemenimScriptModule>(scripts);
        }



        public static void ReloadScripts()
        {
            LoadScripts();
        }
    }
}

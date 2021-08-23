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

        public static ReadOnlyDictionary<string, MemenimScriptModule> Scripts { get; private set; }

        static ScriptManager()
        {
            var baseDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseDirectory) || baseDirectory == "Unknown")
                return;

            var directory = Path.Combine(baseDirectory,
                "scripts");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static void OnInformation(ScriptInformationEventArgs e)
        {
            OnInformation(null, e);
        }
        public static void OnInformation(object sender, ScriptInformationEventArgs e)
        {
            Information?.Invoke(sender, e);
        }

        public static void OnWarning(ScriptWarningEventArgs e)
        {
            OnWarning(null, e);
        }
        public static void OnWarning(object sender, ScriptWarningEventArgs e)
        {
            Warning?.Invoke(sender, e);
        }

        public static void OnError(ScriptErrorEventArgs e)
        {
            OnError(null, e);
        }
        public static void OnError(object sender, ScriptErrorEventArgs e)
        {
            Error?.Invoke(sender, e);
        }


        public static void OnLoaded(ScriptLoadedEventArgs e)
        {
            OnLoaded(null, e);
        }
        public static void OnLoaded(object sender, ScriptLoadedEventArgs e)
        {
            Loaded?.Invoke(sender, e);
        }

        public static void OnUnloaded(ScriptUnloadedEventArgs e)
        {
            OnUnloaded(null, e);
        }
        public static void OnUnloaded(object sender, ScriptUnloadedEventArgs e)
        {
            Unloaded?.Invoke(sender, e);
        }



        private static Dictionary<string, MemenimScriptModule> GetScriptsPaths(
            string directoryBasePath)
        {
            var scriptsPaths = new Dictionary<string, MemenimScriptModule>(10);
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
                    var module = new MemenimScriptModule(
                        directoryPath);

                    scriptsPaths[module.Name] = module;
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
            var scripts = new Dictionary<string, MemenimScriptModule>(10);

            var scriptsDirectoryName = Path.Combine(
                Environment.ExecProcessDirectoryName, "scripts");

            foreach (var script in GetScriptsPaths(
                scriptsDirectoryName))
            {
                scripts[script.Key] = script.Value;
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

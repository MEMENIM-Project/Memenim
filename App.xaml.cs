using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Cryptography;
using Memenim.Dialogs;
using Memenim.Native;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Protocols;
using Memenim.Settings;
using Memenim.Storage;
using Memenim.Utils;
using RIS.Cryptography;
using RIS.Extensions;
using RIS.Logging;
using RIS.Synchronization.Context;
using RIS.Wrappers;
using Environment = RIS.Environment;

namespace Memenim
{
    public partial class App : Application
    {
        private const string UniqueName = "App+MEMENIM+{54FB655C-3F5F-4EE7-823B-10409FD10C7D}";

        private static readonly object InstanceSyncRoot = new object();
        private static volatile SingleInstanceApp _instance;
        internal static SingleInstanceApp Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceSyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SingleInstanceApp(UniqueName);
                        }
                    }
                }

                return _instance;
            }
        }

        private static string AppStartupUri { get; set; }
        private static bool AppCreateHashFiles { get; set; }

        [STAThread]
        private static void Main(string[] args)
        {
            SingleThreadedSynchronizationContext.Await(() =>
            {
                return MainAsync(args);
            });
        }
        private static Task MainAsync(string[] args)
        {
            ParseArgs(args);

            LogManager.Startup();

            if (AppCreateHashFiles)
            {
                CreateHashFiles();

                return Task.CompletedTask;
            }

            return Instance.Run(() =>
            { 
                SingleInstanceMain();
            });
        }

        private static void SingleInstanceMain()
        {
            App app = new App();

            ApiRequestEngine.Information += app.OnCoreInformation;
            ApiRequestEngine.Warning += app.OnCoreWarning;
            ApiRequestEngine.Error += app.OnCoreError;

            app.DispatcherUnhandledException += app.OnDispatcherUnhandledException;

            LogManager.LoggingShutdown += app.LogManager_LoggingShutdown;

            app.InitializeComponent();
            app.Run(Memenim.MainWindow.Instance);
        }

        private static void ParseArgs(string[] args)
        {
            var wrapper = UnwrapArgs(args);

            foreach (var argEntry in wrapper.Enumerate())
            {
                switch (argEntry.Key)
                {
                    case "startupUri":
                        AppStartupUri = (string)argEntry.Value;
                        break;
                    case "createHashFiles":
                        var createHashFilesString = (string)argEntry.Value;

                        try
                        {
                            AppCreateHashFiles = string.IsNullOrWhiteSpace(createHashFilesString)
                                                 || createHashFilesString.ToBoolean();
                        }
                        catch (Exception)
                        {
                            AppCreateHashFiles = false;
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private static void CreateHashFile(string hash, string hashName, string hashType)
        {
            string currentAppVersion = FileVersionInfo
                .GetVersionInfo(Environment.ExecAppAssemblyFilePath)
                .ProductVersion;
            string hashFileNameWithoutExtension = $"{Environment.ExecAppAssemblyFileNameWithoutExtension.Replace('.', '_')}." +
                                                  $"v{currentAppVersion.Replace('.', '_')}." +
                                                  $"{Environment.RuntimeIdentifier.Replace('.', '_')}." +
                                                  $"{(!Environment.IsStandalone ? "!" : string.Empty)}{nameof(Environment.IsStandalone)}." +
                                                  $"{(!Environment.IsSingleFile ? "!" : string.Empty)}{nameof(Environment.IsSingleFile)}";
            string hashFileDirectoryName =
                Path.Combine(Environment.ExecProcessDirectoryName ?? string.Empty, "hash", hashType);

            if (!Directory.Exists(hashFileDirectoryName))
                Directory.CreateDirectory(hashFileDirectoryName);

            using (var file = new StreamWriter(
                Path.Combine(hashFileDirectoryName, $"{hashName}.{hashFileNameWithoutExtension}.{hashType}"),
                false, SecureUtils.SecureUTF8))
            {
                file.WriteLine(hash);
            }
        }



        public static ArgsKeyedWrapper UnwrapArgs(string[] args)
        {
            var argsEntries = new List<(string Key, object Value)>(args.Length);

            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                int separatorPosition = arg.IndexOf(':');

                if (separatorPosition == -1)
                {
                    argsEntries.Add((
                        arg
                            .Trim(' ', '-', '\'', '\"')
                            .TrimStart('-'),
                        string.Empty
                        ));

                    continue;
                }

                argsEntries.Add((
                    arg.Substring(0, separatorPosition)
                        .Trim(' ', '\'', '\"')
                        .TrimStart('-'),
                    arg.Substring(separatorPosition + 1)
                        .Trim(' ', '\'', '\"')
                    ));
            }

            return new ArgsKeyedWrapper(argsEntries);
        }

        public static (string LibrariesHash, string ExeHash, string ExePdbHash) CalculateHashes()
        {
            var librariesHash = HashManager.GetLibrariesHash();
            var exeHash = HashManager.GetExeHash();
            var exePdbHash = HashManager.GetExePdbHash();

            LogManager.Log.Info($"Libraries SHA512 - {librariesHash}");
            LogManager.Log.Info($"Exe SHA512 - {exeHash}");
            LogManager.Log.Info($"Exe PDB SHA512 - {exePdbHash}");

            return (librariesHash, exeHash, exePdbHash);
        }

        public static void CreateHashFiles()
        {
            const string hashType = "sha512";

            LogManager.Log.Info($"Hash file creation started - {hashType}");

            var hashes = CalculateHashes();

            CreateHashFile(hashes.LibrariesHash, "LibrariesHash", hashType);
            CreateHashFile(hashes.ExeHash, "ExeHash", hashType);
            CreateHashFile(hashes.ExePdbHash, "ExePdbHash", hashType);

            LogManager.Log.Info($"Hash files creation completed - {hashType}");
        }



#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        protected override async void OnStartup(StartupEventArgs e)
        {
            await Memenim.MainWindow.Instance.ShowLoadingGrid(true)
                .ConfigureAwait(true);

            MainWindow = Memenim.MainWindow.Instance;

            await Task.Delay(TimeSpan.FromMilliseconds(200))
                .ConfigureAwait(true);

            Memenim.MainWindow.Instance.Show();

            base.OnStartup(e);

            await Task.Delay(TimeSpan.FromMilliseconds(500))
                .ConfigureAwait(true);

            if (Memenim.MainWindow.Instance.Locales.Count == 0)
            {
                var message = LocalizationUtils.TryGetLocalized("NoLocalizationsFoundMessage")
                              ?? "No localizations found";

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                Current.Shutdown(0x1);

                return;
            }

            await StorageManager.Initialize()
                .ConfigureAwait(true);

            ProtocolManager.RegisterAll();

            await Task.Run(async () =>
            {
                LogManager.DeleteLogs(SettingsManager.AppSettings
                    .LogRetentionDaysPeriod);

                try
                {
                    if (string.IsNullOrEmpty(
                        SettingsManager.PersistentSettings.GetCurrentUserLogin()))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });

                        return;
                    }

                    if (!SettingsManager.PersistentSettings.SetCurrentUser(
                        SettingsManager.PersistentSettings.GetCurrentUserLogin()))
                    {
                        SettingsManager.PersistentSettings.RemoveUser(
                            SettingsManager.PersistentSettings.GetCurrentUserLogin());

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });

                        return;
                    }

                    var result = await PostApi.Get(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            PostType.Popular, 1)
                        .ConfigureAwait(false);

                    if (result.IsError
                        && (result.Code == 400 || result.Code == 401))
                    {
                        SettingsManager.PersistentSettings.RemoveUser(
                            SettingsManager.PersistentSettings.CurrentUser.Login);

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });

                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        NavigationController.Instance.RequestPage<FeedPage>();
                    });

                    if (!string.IsNullOrEmpty(AppStartupUri))
                        ProtocolManager.ParseUri(AppStartupUri);
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        NavigationController.Instance.RequestPage<LoginPage>();
                    });
                }
            }).ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(1.5))
                .ConfigureAwait(true);

            await Memenim.MainWindow.Instance.ShowLoadingGrid(false)
                .ConfigureAwait(true);
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        protected override void OnExit(ExitEventArgs e)
        {
            SettingsManager.AppSettings.Save();

            base.OnExit(e);
        }



        private void OnCoreInformation(object sender, CoreInformationEventArgs e)
        {
            //LogManager.DebugLog.Info($"{e.Message}");
        }

        private void OnCoreWarning(object sender, CoreWarningEventArgs e)
        {
            LogManager.Log.Warn($"{e.Message}");
        }

        private void OnCoreError(object sender, CoreErrorEventArgs e)
        {
            LogManager.Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? "Unknown"},HResult={e.SourceException?.HResult.ToString() ?? "Unknown"},StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.Log.Fatal($"{e.Exception?.GetType().Name ?? "Unknown"} - Message={e.Exception?.Message ?? "Unknown"},HResult={e.Exception?.HResult.ToString() ?? "Unknown"},StackTrace=\n{e.Exception?.StackTrace ?? "Unknown"}");
        }



        private void LogManager_LoggingShutdown(object sender, EventArgs e)
        {
            ApiRequestEngine.Information -= OnCoreInformation;
            ApiRequestEngine.Warning -= OnCoreWarning;
            ApiRequestEngine.Error -= OnCoreError;

            DispatcherUnhandledException -= OnDispatcherUnhandledException;
        }
    }
}

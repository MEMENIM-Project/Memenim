using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Cryptography;
using Memenim.Dialogs;
using Memenim.Logging;
using Memenim.Native;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Protocols;
using Memenim.Settings;
using Memenim.Storage;
using Memenim.Utils;
using RIS;
using RIS.Cryptography;
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
        private static bool CreateHashFiles { get; set; }

        [STAThread]
        private static void Main(string[] args)
        {
#pragma warning disable SS002 // DateTime.Now was referenced
            NLog.GlobalDiagnosticsContext.Set("AppStartupTime", DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss", CultureInfo.InvariantCulture));
#pragma warning restore SS002 // DateTime.Now was referenced

            ParseArgs(args);

            Instance.Run(() =>
            {
                SingleInstanceMain();
            });
        }

        private static void SingleInstanceMain()
        {
            LogManager.Log.Info("App Run");

            Events.Information += OnInformation;
            Events.Warning += OnWarning;
            Events.Error += OnError;

            ApiRequestEngine.Information += OnCoreInformation;
            ApiRequestEngine.Warning += OnCoreWarning;
            ApiRequestEngine.Error += OnCoreError;

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.TypeResolve += OnResolve;
            AppDomain.CurrentDomain.ResourceResolve += OnResolve;

            App app = new App();

            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

            LogManager.Log.Info($"Unique Name - {UniqueName}");
            LogManager.Log.Info($"Libraries Directory - {Environment.ExecAppDirectoryName}");
            LogManager.Log.Info($"Execution File Directory - {Environment.ExecProcessDirectoryName}");
            LogManager.Log.Info($"Is Standalone App - {Environment.IsStandalone}");
            LogManager.Log.Info($"Is Single File App - {Environment.IsSingleFile}");
            LogManager.Log.Info($"Runtime Name - {Environment.RuntimeName}");
            LogManager.Log.Info($"Runtime Version - {Environment.RuntimeVersion}");
            LogManager.Log.Info($"Runtime Identifier - {Environment.RuntimeIdentifier}");

            string librariesHash = HashManager.GetLibrariesHash();
            string exeHash = HashManager.GetExeHash();
            string exePdbHash = HashManager.GetExePdbHash();

            LogManager.Log.Info($"Libraries SHA512 - {librariesHash}");
            LogManager.Log.Info($"Exe SHA512 - {exeHash}");
            LogManager.Log.Info($"Exe PDB SHA512 - {exePdbHash}");

            if (CreateHashFiles)
            {
                const string hashType = "sha512";

                LogManager.Log.Info($"Hash file creation started - {hashType}");

                CreateHashFile(librariesHash, "LibrariesHash", hashType);
                CreateHashFile(exeHash, "ExeHash", hashType);
                CreateHashFile(exePdbHash, "ExePdbHash", hashType);

                LogManager.Log.Info($"Hash files creation completed - {hashType}");

                Current.Shutdown(0x0);
                return;
            }

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
                        CreateHashFiles = bool.Parse((string)argEntry.Value);
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
                    argsEntries.Add(
                        (arg.Trim(' ', '\'', '\"'),
                        string.Empty));

                    continue;
                }

                argsEntries.Add(
                    (arg.Substring(0, separatorPosition).Trim(' ', '\'', '\"'),
                    arg.Substring(separatorPosition + 1).Trim(' ', '\'', '\"')));
            }

            return new ArgsKeyedWrapper(argsEntries);
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
                LogManager.Log.Info("Deleted older logs - " +
                                    $"{LogManager.DeleteLogs(Path.Combine(Environment.ExecProcessDirectoryName, "logs"), SettingsManager.AppSettings.LogRetentionDaysPeriod)}");
                LogManager.Log.Info("Deleted older debug logs - " +
                                    $"{LogManager.DeleteLogs(Path.Combine(Environment.ExecProcessDirectoryName, "logs", "debug"), SettingsManager.AppSettings.LogRetentionDaysPeriod)}");

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

            LogManager.Log.Info($"App Exit Code - {e.ApplicationExitCode}");
            NLog.LogManager.Shutdown();

            base.OnExit(e);
        }



        private static void OnInformation(object sender, RInformationEventArgs e)
        {
            LogManager.DebugLog.Info($"{e.Message}");
        }

        private static void OnWarning(object sender, RWarningEventArgs e)
        {
            LogManager.Log.Warn($"{e.Message}");
        }

        private static void OnError(object sender, RErrorEventArgs e)
        {
            LogManager.Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? (e.SourceException?.Message ?? "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private static void OnCoreInformation(object sender, CoreInformationEventArgs e)
        {
            //LogManager.DebugLog.Info($"{e.Message}");
        }

        private static void OnCoreWarning(object sender, CoreWarningEventArgs e)
        {
            LogManager.Log.Warn($"{e.Message}");
        }

        private static void OnCoreError(object sender, CoreErrorEventArgs e)
        {
            LogManager.Log.Error($"{e.SourceException?.GetType().Name ?? "Unknown"} - Message={e.Message ?? (e.SourceException?.Message ?? "Unknown")},HResult={e.SourceException?.HResult ?? 0},StackTrace=\n{e.Stacktrace ?? (e.SourceException?.StackTrace ?? "Unknown")}");
        }



        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            LogManager.Log.Fatal($"{exception?.GetType().Name ?? "Unknown"} - Message={exception?.Message ?? "Unknown"},HResult={exception?.HResult ?? 0},StackTrace=\n{exception?.StackTrace ?? "Unknown"}");
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.Log.Error($"{e.Exception.GetType().Name} - Message={e.Exception.Message},HResult={e.Exception.HResult},StackTrace=\n{e.Exception.StackTrace ?? "Unknown"}");
        }

        private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            LogManager.DebugLog.Error($"{e.Exception.GetType().Name} - Message={e.Exception.Message},HResult={e.Exception.HResult},StackTrace=\n{e.Exception.StackTrace ?? "Unknown"}");
        }



        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            LogManager.DebugLog.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }

        private static Assembly OnResolve(object sender, ResolveEventArgs e)
        {
            LogManager.DebugLog.Info($"Resolve - Name={e.Name ?? "Unknown"},RequestingAssembly={e.RequestingAssembly?.FullName ?? "Unknown"}");

            return e.RequestingAssembly;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Cryptography;
using Memenim.Localization;
using Memenim.Logs;
using Memenim.Native.Window;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Protocols;
using Memenim.Settings;
using Memenim.Storage;
using Memenim.Utils;
using RIS;
using RIS.Extensions;
using RIS.Wrappers;
using Environment = RIS.Environment;

namespace Memenim
{
    public partial class App : Application
    {
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
                            Type appType = typeof(App);

                            _instance =
                                new SingleInstanceApp(appType.Assembly.FullName + appType.FullName
                                                      + "{54FB655C-3F5F-4EE7-823B-10409FD10C7D}");
                        }
                    }
                }

                return _instance;
            }
        }

        private static string AppStartupUri { get; set; }
        private static bool CreateHashFiles { get; set; }

        private static void SingleInstanceMain()
        {
            LogManager.Log.Info("App Run");

            Events.Information += OnInformation;
            Events.Warning += OnWarning;
            Events.Error += OnError;

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.TypeResolve += OnResolve;
            AppDomain.CurrentDomain.ResourceResolve += OnResolve;

            App app = new App();

            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

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
                false, RIS.Cryptography.Utils.SecureUTF8))
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

            Memenim.MainWindow.Instance.Show();

            base.OnStartup(e);

            await LocalizationManager.SwitchLanguage(SettingsManager.AppSettings.Language)
                .ConfigureAwait(true);

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
                    SettingsManager.PersistentSettings.CurrentUserLogin =
                        SettingsManager.PersistentSettings.GetCurrentUserLogin();

                    if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUserLogin))
                    {
                        SettingsManager.PersistentSettings.CurrentUserLogin = null;

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });

                        return;
                    }

                    string userToken = SettingsManager.PersistentSettings.GetUserToken(
                        SettingsManager.PersistentSettings.CurrentUserLogin);
                    string userId = SettingsManager.PersistentSettings.GetUserId(
                        SettingsManager.PersistentSettings.CurrentUserLogin);

                    if (string.IsNullOrEmpty(userToken) || string.IsNullOrEmpty(userId))
                    {
                        SettingsManager.PersistentSettings.CurrentUserLogin = null;

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });

                        return;
                    }

                    SettingsManager.PersistentSettings.CurrentUserToken =
                        PersistentUtils.WinUnprotect(userToken,
                            $"UserToken-{SettingsManager.PersistentSettings.CurrentUserLogin}");
                    SettingsManager.PersistentSettings.CurrentUserId =
                        PersistentUtils.WinUnprotect(userId,
                            $"UserId-{SettingsManager.PersistentSettings.CurrentUserLogin}").ToInt();

                    var resultPosts = await PostApi.Get(
                            SettingsManager.PersistentSettings.CurrentUserToken,
                            PostType.Popular, 1)
                        .ConfigureAwait(false);

                    if (resultPosts.error
                        && (resultPosts.code == 400 || resultPosts.code == 401))
                    {
                        SettingsManager.PersistentSettings.CurrentUserLogin = null;

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });

                        return;
                    }

                    //if (resultPosts.error || resultPosts.data.Count == 0)
                    //{
                    //    Dispatcher.Invoke(() =>
                    //    {
                    //        NavigationController.Instance.RequestPage<LoginPage>();
                    //    });

                    //    return;
                    //}

                    //var resultPost = resultPosts.data[0];

                    //for (int i = 0; i < 2; ++i)
                    //{
                    //    ApiResponse<CountSchema> result;

                    //    if (resultPost.likes.my == 0)
                    //    {
                    //        result = await PostApi.AddLike(
                    //                SettingsManager.PersistentSettings.CurrentUserToken,
                    //                resultPost.id)
                    //            .ConfigureAwait(false);
                    //    }
                    //    else
                    //    {
                    //        result = await PostApi.RemoveLike(
                    //                SettingsManager.PersistentSettings.CurrentUserToken,
                    //                resultPost.id)
                    //            .ConfigureAwait(false);
                    //    }

                    //    if (result.error
                    //        && (result.code == 400 || result.code == 401))
                    //    {
                    //        Dispatcher.Invoke(() =>
                    //        {
                    //            NavigationController.Instance.RequestPage<LoginPage>();
                    //        });

                    //        return;
                    //    }

                    //    if (resultPost.likes.my == 0)
                    //        ++resultPost.likes.my;
                    //    else
                    //        --resultPost.likes.my;
                    //}

                    Dispatcher.Invoke(() =>
                    {
                        NavigationController.Instance.RequestPage<FeedPage>();

                        if (!string.IsNullOrEmpty(AppStartupUri))
                            ProtocolManager.ParseUri(AppStartupUri);
                    });
                }
                catch (CryptographicException)
                {
                    SettingsManager.PersistentSettings.CurrentUserLogin = null;

                    Dispatcher.Invoke(() =>
                    {
                        NavigationController.Instance.RequestPage<LoginPage>();
                    });
                }
            }).ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(1))
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
            LogManager.Log.Info($"{e.Message}");
        }

        private static void OnWarning(object sender, RWarningEventArgs e)
        {
            LogManager.Log.Warn($"{e.Message}");
        }

        private static void OnError(object sender, RErrorEventArgs e)
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

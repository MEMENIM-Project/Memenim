using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
using Memenim.SpecialEvents;
using Memenim.Storage;
using Memenim.Utils;
using RIS.Cryptography;
using RIS.Extensions;
using RIS.Localization;
using RIS.Localization.Providers;
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
            var app = new App();

            ApiRequestEngine.Information += app.OnCoreInformation;
            ApiRequestEngine.Warning += app.OnCoreWarning;
            ApiRequestEngine.Error += app.OnCoreError;

            app.DispatcherUnhandledException += app.OnDispatcherUnhandledException;

            LogManager.LoggingShutdown += app.LogManager_LoggingShutdown;

            LocalizationUtils.Initialize();

            LocalizationUtils.LocalizationsLoaded += app.LocalizationUtils_LocalizationsLoaded;
            LocalizationUtils.LocalizationsNotFound += app.LocalizationUtils_LocalizationsNotFound;
            LocalizationUtils.LocalizationFileNotFound += app.LocalizationUtils_LocalizationFileNotFound;
            LocalizationUtils.LocalizedCultureNotFound += app.LocalizationUtils_LocalizedCultureNotFound;

            app.InitializeComponent();
            app.Run(Memenim.MainWindow.Instance);
        }



        private static void ParseArgs(
            string[] args)
        {
            var wrapper = UnwrapArgs(
                args);

            foreach (var (key, value) in wrapper.Enumerate())
            {
                switch (key)
                {
                    case "startupUri":
                        AppStartupUri = (string)value;

                        break;
                    case "createHashFiles":
                        var createHashFilesString = (string)value;

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

        private static void CreateHashFile(
            string hash, string hashName,
            string hashType)
        {
            var currentAppVersion = FileVersionInfo
                .GetVersionInfo(Environment.ExecAppAssemblyFilePath)
                .ProductVersion;
            var hashFileNameWithoutExtension = $"{Environment.ExecAppAssemblyFileNameWithoutExtension.Replace('.', '_')}." +
                                                  $"v{currentAppVersion.Replace('.', '_')}." +
                                                  $"{Environment.RuntimeIdentifier.Replace('.', '_')}." +
                                                  $"{(!Environment.IsStandalone ? "!" : string.Empty)}{nameof(Environment.IsStandalone)}." +
                                                  $"{(!Environment.IsSingleFile ? "!" : string.Empty)}{nameof(Environment.IsSingleFile)}";
            var hashFileDirectoryName =
                Path.Combine(
                    Environment.ExecProcessDirectoryName ?? string.Empty,
                    "hash",
                    hashType);

            if (!Directory.Exists(hashFileDirectoryName))
                Directory.CreateDirectory(hashFileDirectoryName);

            using var file = new StreamWriter(
                Path.Combine(
                    hashFileDirectoryName,
                    $"{hashName}.{hashFileNameWithoutExtension}.{hashType}"),
                false, SecureUtils.SecureUTF8);

            file.WriteLine(hash);
        }




        public static ArgsKeyedWrapper UnwrapArgs(
            string[] args)
        {
            var argsEntries = new List<(string Key, object Value)>(args.Length);

            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                    continue;

                var separatorPosition = arg
                    .IndexOf(':');

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
                    arg[..separatorPosition]
                        .Trim(' ', '\'', '\"')
                        .TrimStart('-'),
                    arg[(separatorPosition + 1)..]
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

            LogManager.Default.Info($"Libraries SHA512 - {librariesHash}");
            LogManager.Default.Info($"Exe SHA512 - {exeHash}");
            LogManager.Default.Info($"Exe PDB SHA512 - {exePdbHash}");

            return (librariesHash, exeHash, exePdbHash);
        }

        public static void CreateHashFiles()
        {
            const string hashType = "sha512";

            LogManager.Default.Info($"Hash file creation started - {hashType}");

            var (librariesHash, exeHash, exePdbHash) = CalculateHashes();

            CreateHashFile(librariesHash, "LibrariesHash", hashType);
            CreateHashFile(exeHash, "ExeHash", hashType);
            CreateHashFile(exePdbHash, "ExePdbHash", hashType);

            LogManager.Default.Info($"Hash files creation completed - {hashType}");
        }



#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        protected override async void OnStartup(
            StartupEventArgs e)
        {
            await Memenim.MainWindow.Instance
                .ShowLoadingGrid()
                .ConfigureAwait(true);

            MainWindow = Memenim.MainWindow.Instance;

            await Task.Delay(
                    TimeSpan.FromSeconds(0.2))
                .ConfigureAwait(true);

            Memenim.MainWindow.Instance.Show();

            base.OnStartup(e);

            await Task.Delay(
                    TimeSpan.FromSeconds(0.5))
                .ConfigureAwait(true);

            LocalizationUtils.ReloadLocalizations<XamlLocalizationProvider>();

            if (LocalizationUtils.Localizations.Count == 0)
                return;

            LocalizationUtils.SetDefaultCulture(
                "en-US");
            LocalizationUtils.SwitchLocalization(
                SettingsManager.AppSettings.Language);

            LocalizationUtils.LocalizationChanged += LocalizationUtils_LocalizationChanged;

            await Task.Run(async () =>
            {
                ProtocolManager.RegisterAll();

                await StorageManager.Initialize()
                    .ConfigureAwait(false);

                LogManager.DeleteLogs(SettingsManager.AppSettings
                    .LogRetentionDaysPeriod);

                try
                {
                    var currentUserLogin =
                        SettingsManager.PersistentSettings.GetCurrentUserLogin();

                    if (string.IsNullOrEmpty(currentUserLogin))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance
                                .RequestPage<LoginPage>();
                        });

                        return;
                    }

                    if (!SettingsManager.PersistentSettings.SetCurrentUser(currentUserLogin))
                    {
                        SettingsManager.PersistentSettings.RemoveUser(
                            currentUserLogin);

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance
                                .RequestPage<LoginPage>();
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
                            NavigationController.Instance
                                .RequestPage<LoginPage>();
                        });

                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        NavigationController.Instance
                            .RequestPage<FeedPage>();
                    });

                    if (!string.IsNullOrEmpty(AppStartupUri))
                        ProtocolManager.ParseUri(AppStartupUri);
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() =>
                    {
                        NavigationController.Instance
                            .RequestPage<LoginPage>();
                    });
                }
            }).ConfigureAwait(true);

            SpecialEventManager.UpdateEvent();

            await Task.Delay(
                    TimeSpan.FromSeconds(1.5))
                .ConfigureAwait(true);

            await Memenim.MainWindow.Instance
                .HideLoadingGrid()
                .ConfigureAwait(true);
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        protected override void OnExit(
            ExitEventArgs e)
        {
            SettingsManager.AppSettings.Save();

            base.OnExit(e);
        }



        private void OnCoreInformation(object sender,
            CoreInformationEventArgs e)
        {
            //LogManager.Debug.Info(
            //    $"{e.Message}");
        }

        private void OnCoreWarning(object sender,
            CoreWarningEventArgs e)
        {
            LogManager.Default.Warn(
                $"{e.Message}");
        }

        private void OnCoreError(object sender,
            CoreErrorEventArgs e)
        {
            LogManager.Default.Error(
                $"{e.SourceException?.GetType().Name ?? "Unknown"} - " +
                $"Message={e.Message ?? "Unknown"}," +
                $"HResult={e.SourceException?.HResult.ToString() ?? "Unknown"}," +
                $"StackTrace=\n{e.SourceException?.StackTrace ?? "Unknown"}");
        }



        private void OnDispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.Default.Fatal(
                $"{e.Exception?.GetType().Name ?? "Unknown"} - " +
                $"Message={e.Exception?.Message ?? "Unknown"}," +
                $"HResult={e.Exception?.HResult.ToString() ?? "Unknown"}," +
                $"StackTrace=\n{e.Exception?.StackTrace ?? "Unknown"}");
        }



        private void LogManager_LoggingShutdown(object sender,
            EventArgs e)
        {
            ApiRequestEngine.Information -= OnCoreInformation;
            ApiRequestEngine.Warning -= OnCoreWarning;
            ApiRequestEngine.Error -= OnCoreError;

            DispatcherUnhandledException -= OnDispatcherUnhandledException;
        }



        private void LocalizationUtils_LocalizationChanged(object sender,
            LocalizationChangedEventArgs e)
        {
            Current?.Dispatcher?.Invoke(() =>
            {
                Thread.CurrentThread.CurrentCulture = e.NewLocalization?.Culture
                                                      ?? LocalizationUtils.DefaultCulture;
                Thread.CurrentThread.CurrentUICulture = e.NewLocalization?.Culture
                                                        ?? LocalizationUtils.DefaultCulture;
            });

            SettingsManager.AppSettings.Language = e.NewLocalization.CultureName;

            SettingsManager.AppSettings.Save();
        }

        private async void LocalizationUtils_LocalizationsLoaded(object sender,
            EventArgs e)
        {
            if (LocalizationUtils.Localizations.Count > 0)
                return;

            LocalizationUtils.TryGetLocalized(
                "LocalizationsNotFoundMessage", out var message);

            if (string.IsNullOrEmpty(message))
                message = "Localizations not found";

            await DialogManager.ShowErrorDialog(message)
                .ConfigureAwait(true);

            Current.Shutdown(0x1);
        }

        private async void LocalizationUtils_LocalizationsNotFound(object sender,
            EventArgs e)
        {
            LocalizationUtils.TryGetLocalized(
                "LocalizationsNotFoundMessage", out var message);

            if (string.IsNullOrEmpty(message))
                message = "Localizations not found";

            await DialogManager.ShowErrorDialog(message)
                .ConfigureAwait(true);

            Current.Shutdown(0x1);
        }

        private async void LocalizationUtils_LocalizationFileNotFound(object sender,
            LocalizationFileNotFoundEventArgs e)
        {
            LocalizationUtils.TryGetLocalized(
                "LocalizationFileTitle", out var localizationFileTitle);

            if (string.IsNullOrEmpty(localizationFileTitle))
                localizationFileTitle = "Localization file";

            LocalizationUtils.TryGetLocalized(
                "NotFoundTitle1", out var notFoundTitle);

            if (string.IsNullOrEmpty(notFoundTitle))
                notFoundTitle = "Not found";

            await DialogManager.ShowErrorDialog(
                    $"{localizationFileTitle}['{e.FilePath}'] {notFoundTitle.ToLower()}")
                .ConfigureAwait(true);
        }

        private async void LocalizationUtils_LocalizedCultureNotFound(object sender,
            LocalizedCultureNotFoundEventArgs e)
        {
            LocalizationUtils.TryGetLocalized(
                "LocalizedCultureTitle", out var localizedCultureTitle);

            if (string.IsNullOrEmpty(localizedCultureTitle))
                localizedCultureTitle = "Localized culture";

            LocalizationUtils.TryGetLocalized(
                "NotFoundTitle2", out var notFoundTitle);

            if (string.IsNullOrEmpty(notFoundTitle))
                notFoundTitle = "Not found";

            await DialogManager.ShowErrorDialog(
                    $"{localizedCultureTitle}['{e.CultureName}'] {notFoundTitle.ToLower()}")
                .ConfigureAwait(true);
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Localization;
using Memenim.Native.Window;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Settings;
using Memenim.Utils;
using RIS.Extensions;

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

        private static void SingleInstanceMain(string[] args)
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        [STAThread]
        private static void Main(string[] args)
        {
            Instance.Run(() =>
            {
                SingleInstanceMain(args);
            });
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        protected override async void OnStartup(StartupEventArgs e)
        {
            await Memenim.MainWindow.Instance.ShowLoadingGrid(true)
                .ConfigureAwait(true);

            MainWindow = Memenim.MainWindow.Instance;

            Memenim.MainWindow.Instance.Show();

            base.OnStartup(e);

            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(true);

            await LocalizationManager.SwitchLanguage(SettingsManager.AppSettings.Language)
                .ConfigureAwait(true);

            await Task.Run(() =>
            {
                try
                {
                    SettingsManager.PersistentSettings.CurrentUserLogin =
                        SettingsManager.PersistentSettings.GetCurrentUserLogin();

                    if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUserLogin))
                    {
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

                    if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(userId))
                    {
                        SettingsManager.PersistentSettings.CurrentUserToken =
                            PersistentUtils.WinUnprotect(userToken,
                                $"UserToken-{SettingsManager.PersistentSettings.CurrentUserLogin}");
                        SettingsManager.PersistentSettings.CurrentUserId =
                            PersistentUtils.WinUnprotect(userId,
                                $"UserId-{SettingsManager.PersistentSettings.CurrentUserLogin}").ToInt();

                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<FeedPage>();
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NavigationController.Instance.RequestPage<LoginPage>();
                        });
                    }
                }
                catch (CryptographicException)
                {
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

            base.OnExit(e);
        }
    }
}

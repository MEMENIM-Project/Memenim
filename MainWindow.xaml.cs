using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Localization;
using Memenim.Native.Window;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Settings;
using Math = RIS.Mathematics.Math;

namespace Memenim
{
    public sealed partial class MainWindow : MetroWindow, INativeRestorableWindow
    {
        private static readonly object InstanceSyncRoot = new object();
        private static volatile MainWindow _instance;
        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceSyncRoot)
                    {
                        if (_instance == null)
                            _instance = new MainWindow();
                    }
                }

                return _instance;
            }
        }

        private HwndSource _hwndSource;
        private WindowState _previousState;

        public ReadOnlyDictionary<string, string> Locales { get; private set; }
        public string AppVersion { get; private set; }
        public bool DuringRestoreToMaximized { get; private set; }

        private MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _instance = this;

            RootLayout.Children.Add(NavigationController.Instance);

            Width = SettingsManager.AppSettings.WindowWidth;
            Height = SettingsManager.AppSettings.WindowHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SettingsManager.AppSettings.WindowPositionX - (Width / 2.0);
            Top = SettingsManager.AppSettings.WindowPositionY - (Height / 2.0);
            WindowState = (WindowState)SettingsManager.AppSettings.WindowState;

            _previousState = WindowState;
            DuringRestoreToMaximized = WindowState == WindowState.Maximized;

            AppVersion = $"v{SettingsManager.AppSettings.AppVersion}";

            LocalizationManager.ReloadLocales();

            if (Locales.Count == 0)
                return;

            if (Locales.TryGetValue(SettingsManager.AppSettings.Language, out var localeName))
            {
                slcLanguage.SelectedItem = new KeyValuePair<string, string>(
                    SettingsManager.AppSettings.Language, localeName);
            }
            else if (Locales.TryGetValue("en-US", out localeName))
            {
                slcLanguage.SelectedItem = new KeyValuePair<string, string>(
                    "en-US", localeName);

                SettingsManager.AppSettings.Language = "en-US";
                SettingsManager.AppSettings.Save();
            }
            else
            {
                var locale = Locales.First();

                slcLanguage.SelectedItem = locale;

                SettingsManager.AppSettings.Language = locale.Key;
                SettingsManager.AppSettings.Save();
            }
        }

        public bool IsOpenSettings()
        {
            return SettingsFlyout.IsOpen;
        }

        public void ShowSettings()
        {
            SettingsFlyout.IsOpen = true;
        }

        public void HideSettings()
        {
            SettingsFlyout.IsOpen = false;
        }

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        loadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    loadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwndSource = (HwndSource) PresentationSource.FromVisual(this);
            _hwndSource?.AddHook(WindowUtils.HwndSourceHook);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            _previousState = WindowState;

            base.OnStateChanged(e);

            if (_previousState != WindowState.Minimized)
            {
                DuringRestoreToMaximized = WindowState == WindowState.Maximized;
            }
        }

        private async void slcLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPair = (KeyValuePair<string, string>)slcLanguage.SelectedItem;

            await LocalizationManager.SwitchLanguage(selectedPair.Key)
                .ConfigureAwait(true);
        }

        private async void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string changePasswordLocalizeName = (string)Instance
                .FindResource("ChangePassword");
            string enterLocalizeName = (string)Instance
                .FindResource("EnterTitle");
            string oldPasswordLocalizeName = (string)Instance
                .FindResource("OldPassword");
            string newPasswordLocalizeName = (string)Instance
                .FindResource("NewPassword");

            string oldPassword = await DialogManager.ShowPasswordDialog(changePasswordLocalizeName,
                    $"{enterLocalizeName} {oldPasswordLocalizeName.ToLower()}",
                    false)
                .ConfigureAwait(true);

            if (oldPassword == null)
                return;

            string newPassword = await DialogManager.ShowPasswordDialog(changePasswordLocalizeName,
                    $"{enterLocalizeName} {newPasswordLocalizeName.ToLower()}",
                    true)
                .ConfigureAwait(true);

            if (newPassword == null)
                return;

            var request = await UserApi.ChangePassword(
                    SettingsManager.PersistentSettings.CurrentUserToken,
                    oldPassword, newPassword)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);
            }
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.PersistentSettings.RemoveUser(
                SettingsManager.PersistentSettings.CurrentUserLogin);

            HideSettings();

            NavigationController.Instance.RequestPage<LoginPage>();
        }

        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            //var startInfo = new ProcessStartInfo
            //{
            //    FileName = "cmd",
            //    Arguments = "/C start https://discord.gg/yfSrUwCmZ8",
            //    WindowStyle = ProcessWindowStyle.Hidden,
            //    CreateNoWindow = true,
            //    UseShellExecute = false
            //};

            var startInfo = new ProcessStartInfo
            {
                FileName = "https://discord.gg/yfSrUwCmZ8",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SettingsManager.AppSettings.WindowWidth = Width;
            SettingsManager.AppSettings.WindowHeight = Height;
            SettingsManager.AppSettings.WindowPositionX = Left + (Width / 2.0);
            SettingsManager.AppSettings.WindowPositionY = Top + (Height / 2.0);
            SettingsManager.AppSettings.WindowState = (int)WindowState;

            SettingsManager.AppSettings.Save();
        }
    }
}

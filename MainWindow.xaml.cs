using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Localization;
using Memenim.Localization.Entities;
using Memenim.Misc;
using Memenim.Native.Window;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Settings;
using Memenim.Utils;
using Math = RIS.Mathematics.Math;

namespace Memenim
{
    public sealed partial class MainWindow : MetroWindow, ILocalizable, INativeRestorableWindow
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

        private int _loadingGridStatus;
        public bool LoadingGridStatus
        {
            get
            {
                return _loadingGridStatus > 0;
            }
            private set
            {
                Interlocked.Exchange(ref _loadingGridStatus, value ? 1 : 0);
            }
        }
        private Task _loadingGridTask;
        private int _connectionFailedGridStatus;
        public bool ConnectionFailedGridStatus
        {
            get
            {
                return _connectionFailedGridStatus > 0;
            }
            private set
            {
                Interlocked.Exchange(ref _connectionFailedGridStatus, value ? 1 : 0);
            }
        }
        private Task _connectionFailedGridTask;

        private bool _specialEventEnabled;
        public bool SpecialEventEnabled
        {
            get
            {
                return _specialEventEnabled;
            }
            set
            {
                SpecialEventLayer.Instance.Activate(value);

                _specialEventEnabled = value;
            }
        }
        private double _bgmVolume;
        public double BgmVolume
        {
            get
            {
                return _bgmVolume;
            }
            set
            {
                SpecialEventLayer.Instance.SetVolume(value);

                _bgmVolume = value;
            }
        }
        public ReadOnlyDictionary<string, LocalizationXamlFile> Locales { get; private set; }
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

            LoadingGridStatus = false;
            _loadingGridTask = Task.CompletedTask;
            ConnectionFailedGridStatus = false;
            _connectionFailedGridTask = Task.CompletedTask;

            LoadSpecialEvent();

            LocalizationManager.ReloadLocales();

            if (Locales.Count == 0)
                return;

            LocalizationManager.SetDefaultLanguage()
                .ConfigureAwait(true);

            if (Locales.TryGetValue(SettingsManager.AppSettings.Language, out var localizationFile))
            {
                slcLanguage.SelectedItem = new KeyValuePair<string, LocalizationXamlFile>(
                    SettingsManager.AppSettings.Language, localizationFile);
            }
            else if (Locales.TryGetValue("en-US", out localizationFile))
            {
                slcLanguage.SelectedItem = new KeyValuePair<string, LocalizationXamlFile>(
                    "en-US", localizationFile);

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

            ApiRequestEngine.ConnectionStateChanged += OnConnectionStateChanged;
        }

        ~MainWindow()
        {
            ApiRequestEngine.ConnectionStateChanged -= OnConnectionStateChanged;
        }

        private void LoadSpecialEvent()
        {
            var appStartupTime = DateTime.ParseExact(
                NLog.GlobalDiagnosticsContext.Get("AppStartupTime"),
                "yyyy.MM.dd HH-mm-ss", CultureInfo.InvariantCulture);
            var eventStartTime = DateTime.ParseExact("0001.12.20 00-00-00",
                    "yyyy.MM.dd HH-mm-ss", CultureInfo.InvariantCulture)
                .AddYears(appStartupTime.Year - 1);
            var eventEndTime = DateTime.ParseExact("0001.01.10 23-59-59",
                    "yyyy.MM.dd HH-mm-ss", CultureInfo.InvariantCulture)
                .AddYears(appStartupTime.Year - 1);

            if (!(eventStartTime <= appStartupTime
                  || appStartupTime <= eventEndTime))
            {
                return;
            }

            RootLayout.Children.Add(SpecialEventLayer.Instance);

            SpecialEventPanel.Visibility = Visibility.Visible;

            SpecialEventEnabled = SettingsManager.AppSettings.SpecialEventEnabled;
            BgmVolume = SettingsManager.AppSettings.BgmVolume;
        }

        public void LinkOpenEnable(bool isEnabled)
        {
            if (NavigationController.Instance.RootLayout.DisplayMode == SplitViewDisplayMode.Inline
                || loadingGrid.Visibility == Visibility.Visible
                || connectionFailedGrid.Visibility == Visibility.Visible)
            {
                btnLinkOpen.IsEnabled = false;
                return;
            }

            btnLinkOpen.IsEnabled = isEnabled;
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

        public async Task ShowLoadingGrid(bool status)
        {
            await _loadingGridTask
                .ConfigureAwait(true);

            var oldStatus = Interlocked.Exchange(ref _loadingGridStatus, status ? 1 : 0);

            if (oldStatus == (status ? 1 : 0))
                return;

            if (status)
            {
                LinkOpenEnable(false);
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                _loadingGridTask = Task.CompletedTask;
                return;
            }

            loadingIndicator.IsActive = false;

            _loadingGridTask = Task.Run(async () =>
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
                    LinkOpenEnable(true);
                });
            });
        }

        public async Task ShowConnectionFailedGrid(bool status)
        {
            await _connectionFailedGridTask
                .ConfigureAwait(true);

            var oldStatus = Interlocked.Exchange(ref _connectionFailedGridStatus, status ? 1 : 0);

            if (oldStatus == (status ? 1 : 0))
                return;

            if (status)
            {
                LinkOpenEnable(false);
                connectionFailedIndicator.IsActive = true;
                connectionFailedGrid.Opacity = 1.0;
                connectionFailedGrid.IsHitTestVisible = true;
                connectionFailedGrid.Visibility = Visibility.Visible;

                _connectionFailedGridTask = Task.CompletedTask;
                return;
            }

            connectionFailedIndicator.IsActive = false;

            _connectionFailedGridTask = Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            connectionFailedGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        connectionFailedGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    connectionFailedGrid.Visibility = Visibility.Collapsed;
                    LinkOpenEnable(true);
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

        private async void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            await Dispatcher.Invoke(() =>
            {
                return e.NewState switch
                {
                    ConnectionStateType.Connected => ShowConnectionFailedGrid(false),
                    ConnectionStateType.Disconnected => ShowConnectionFailedGrid(true),
                    _ => Task.CompletedTask,
                };
            }).ConfigureAwait(true);
        }

        private async void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (!btnLinkOpen.IsEnabled)
                return;

            string title = LocalizationUtils.GetLocalized("LinkOpeningTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");

            string link = await DialogManager.ShowSinglelineTextDialog(
                    title, $"{enterName} URL")
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(link))
                return;

            var startInfo = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        private async void slcLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPair = (KeyValuePair<string, LocalizationXamlFile>)slcLanguage.SelectedItem;

            await LocalizationManager.SwitchLanguage(selectedPair.Key)
                .ConfigureAwait(true);
        }

        private async void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string title = LocalizationUtils.GetLocalized("ChangingPasswordTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");
            string oldPasswordName = LocalizationUtils.GetLocalized("OldPassword");
            string newPasswordName = LocalizationUtils.GetLocalized("NewPassword");

            string oldPassword = await DialogManager.ShowPasswordDialog(title,
                    $"{enterName} {oldPasswordName.ToLower()}",
                    false)
                .ConfigureAwait(true);

            if (oldPassword == null)
                return;

            string newPassword = await DialogManager.ShowPasswordDialog(title,
                    $"{enterName} {newPasswordName.ToLower()}",
                    true)
                .ConfigureAwait(true);

            if (newPassword == null)
                return;

            var request = await UserApi.ChangePassword(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    oldPassword, newPassword)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowErrorDialog(request.message)
                    .ConfigureAwait(true);
            }
        }

        private async void btnSignInToAnotherAccount_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsManager.PersistentSettings.CurrentUser.IsTemporary())
            {
                var confirmResult = await DialogManager.ShowConfirmationDialog()
                    .ConfigureAwait(true);

                if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                {
                    return;
                }
            }

            SettingsManager.PersistentSettings.ResetCurrentUser();

            HideSettings();

            NavigationController.Instance.RequestPage<LoginPage>();
        }

        private async void btnSignOutAccount_Click(object sender, RoutedEventArgs e)
        {
            var confirmResult = await DialogManager.ShowConfirmationDialog()
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                return;
            }

            SettingsManager.PersistentSettings.RemoveUser(
                SettingsManager.PersistentSettings.CurrentUser.Login);

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

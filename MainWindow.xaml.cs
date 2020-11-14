using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Memenim.Settings;
using MahApps.Metro.Controls;
using Memenim.Native.Window;
using Memenim.Navigation;

namespace Memenim
{
    public sealed partial class MainWindow : MetroWindow, INativeRestorableWindow
    {
        private static object InstanceSyncRoot = new object();
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

                    if (opacity == 0.7)
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

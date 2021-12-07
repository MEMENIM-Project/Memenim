using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Native.Window;
using Memenim.Navigation;
using Memenim.Settings;
using Memenim.Styles;
using Memenim.Styles.Loading.Entities;
using Memenim.Utils;
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


        private int _titleLockStatus;
        public bool TitleLockStatus
        {
            get
            {
                return _titleLockStatus > 0;
            }
            private set
            {
                Interlocked.Exchange(ref _titleLockStatus, value ? 1 : 0);
            }
        }
        private bool _loadingGridTitleLockStatus;
        private bool _connectionFailedGridTitleLockStatus;


        private LoadingStyle _loadingStyle;



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

            LoadingGridStatus = false;
            _loadingGridTask = Task.CompletedTask;
            ConnectionFailedGridStatus = false;
            _connectionFailedGridTask = Task.CompletedTask;

            ApplyLoadingStyle();

            ApiRequestEngine.ConnectionStateChanged += OnConnectionStateChanged;
        }

        ~MainWindow()
        {
            ApiRequestEngine.ConnectionStateChanged -= OnConnectionStateChanged;
        }



        private void ApplyLoadingStyle()
        {
            _loadingStyle = StylesManager.GetRandomLoadingStyle();

            if (_loadingStyle.ForegroundImageUri != null)
            {

                LoadingForegroundImage.Source = new BitmapImage(
                    _loadingStyle.ForegroundImageUri);
                ConnectionFailedForegroundImage.Source = new BitmapImage(
                    _loadingStyle.ForegroundImageUri);

                LoadingForegroundImage.Visibility = Visibility.Visible;
                ConnectionFailedForegroundImage.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingForegroundImage.Source = null;
                ConnectionFailedForegroundImage.Source = null;

                LoadingForegroundImage.Visibility = Visibility.Hidden;
                ConnectionFailedForegroundImage.Visibility = Visibility.Hidden;
            }

            if (_loadingStyle.BackgroundImageUri != null)
            {

                LoadingBackgroundImage.Source = new BitmapImage(
                    _loadingStyle.BackgroundImageUri);
                ConnectionFailedBackgroundImage.Source = new BitmapImage(
                    _loadingStyle.BackgroundImageUri);

                LoadingBackgroundImage.Visibility = Visibility.Visible;
                ConnectionFailedBackgroundImage.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingBackgroundImage.Source = null;
                ConnectionFailedBackgroundImage.Source = null;

                LoadingBackgroundImage.Visibility = Visibility.Hidden;
                ConnectionFailedBackgroundImage.Visibility = Visibility.Hidden;
            }

            LoadingIndicator.HorizontalAlignment = _loadingStyle.LoadingIndicatorHorizontalAlignment;
            LoadingIndicator.VerticalAlignment= _loadingStyle.LoadingIndicatorVerticalAlignment;

            ConnectionFailedIndicator.HorizontalAlignment = _loadingStyle.LoadingIndicatorHorizontalAlignment;
            ConnectionFailedIndicator.VerticalAlignment = _loadingStyle.LoadingIndicatorVerticalAlignment;
        }



        public void ActivateOpenLink()
        {
            if (NavigationController.Instance.RootLayout.DisplayMode == SplitViewDisplayMode.Inline
                || LoadingGrid.Visibility == Visibility.Visible
                || ConnectionFailedGrid.Visibility == Visibility.Visible)
            {
                return;
            }

            OpenLinkButton.IsEnabled = true;
        }

        public void DeactivateOpenLink()
        {
            OpenLinkButton.IsEnabled = false;
        }



        public async Task ShowLoadingGrid()
        {
            await _loadingGridTask
                .ConfigureAwait(true);

            const int newStatus = 1;
            var oldStatus = Interlocked.Exchange(
                ref _loadingGridStatus, newStatus);

            if (oldStatus == newStatus)
                return;

            bool changeStyleNeeded;

            if (!TitleLockStatus)
            {
                TitleLockStatus = true;
                _loadingGridTitleLockStatus = true;

                changeStyleNeeded = true;
            }
            else
            {
                changeStyleNeeded = false;
            }

            DeactivateOpenLink();

            LoadingIndicator.IsActive = true;
            LoadingGrid.Opacity = 1.0;
            LoadingGrid.IsHitTestVisible = true;
            LoadingGrid.Visibility = Visibility.Visible;

            if (changeStyleNeeded)
            {
                Resources.MergedDictionaries
                    .Add(_loadingStyle.Dictionary);
                Resources.MergedDictionaries
                    .Add(_loadingStyle.MahAppsDictionary);
            }

            _loadingGridTask = Task.CompletedTask;
        }

        public async Task HideLoadingGrid()
        {
            await _loadingGridTask
                .ConfigureAwait(true);

            const int newStatus = 0;
            var oldStatus = Interlocked.Exchange(
                ref _loadingGridStatus, newStatus);

            if (oldStatus == newStatus)
                return;

            bool changeStyleNeeded;

            if (!TitleLockStatus)
            {
                TitleLockStatus = true;
                _loadingGridTitleLockStatus = true;

                changeStyleNeeded = true;
            }
            else
            {
                changeStyleNeeded = false;
            }

            LoadingIndicator.IsActive = false;

            if (_loadingGridTitleLockStatus)
                changeStyleNeeded = true;

            var color = ((SolidColorBrush)FindResource("Window.Main.TitleBackground"))
                .Color;

            _loadingGridTask = Task.Run(async () =>
            {
                Action<double> changeOpacityFunction;

                if (changeStyleNeeded)
                {
                    changeOpacityFunction = opacity =>
                    {
                        LoadingGrid.Opacity = opacity;

                        var brush = new SolidColorBrush(
                            Color.FromArgb((byte)(255 * opacity),
                                color.R, color.G, color.B));

                        WindowTitleBrush = brush;
                        NonActiveWindowTitleBrush = brush;
                    };
                }
                else
                {
                    changeOpacityFunction = opacity =>
                    {
                        LoadingGrid.Opacity = opacity;
                    };
                }

                var isHitTestVisible = true;

                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (isHitTestVisible
                        && Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingGrid.IsHitTestVisible = false;

                            Resources.MergedDictionaries
                                .Remove(_loadingStyle.MahAppsDictionary);
                        });

                        isHitTestVisible = false;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        changeOpacityFunction(opacity);
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    if (changeStyleNeeded)
                    {
                        Resources.MergedDictionaries
                            .Remove(_loadingStyle.Dictionary);

                        SetResourceReference(WindowTitleBrushProperty,
                            "Window.Main.TitleBackground");
                        SetResourceReference(NonActiveWindowTitleBrushProperty,
                            "Window.Main.NonActiveTitleBackground");
                    }

                    LoadingGrid.IsHitTestVisible = false;
                    LoadingGrid.Visibility = Visibility.Collapsed;

                    ActivateOpenLink();

                    if (changeStyleNeeded)
                    {
                        TitleLockStatus = false;
                        _loadingGridTitleLockStatus = false;
                    }
                });
            });
        }

        public async Task ShowConnectionFailedGrid()
        {
            await _connectionFailedGridTask
                .ConfigureAwait(true);

            const int newStatus = 1;
            var oldStatus = Interlocked.Exchange(
                ref _connectionFailedGridStatus, newStatus);

            if (oldStatus == newStatus)
                return;

            bool changeStyleNeeded;

            if (!TitleLockStatus)
            {
                TitleLockStatus = true;
                _connectionFailedGridTitleLockStatus = true;

                changeStyleNeeded = true;
            }
            else
            {
                changeStyleNeeded = false;
            }

            DeactivateOpenLink();

            ConnectionFailedIndicator.IsActive = true;
            ConnectionFailedGrid.Opacity = 1.0;
            ConnectionFailedGrid.IsHitTestVisible = true;
            ConnectionFailedGrid.Visibility = Visibility.Visible;

            if (changeStyleNeeded)
            {
                Resources.MergedDictionaries
                    .Add(_loadingStyle.Dictionary);
                Resources.MergedDictionaries
                    .Add(_loadingStyle.MahAppsDictionary);
            }

            _connectionFailedGridTask = Task.CompletedTask;
        }

        public async Task HideConnectionFailedGrid()
        {
            await _connectionFailedGridTask
                .ConfigureAwait(true);

            const int newStatus = 0;
            var oldStatus = Interlocked.Exchange(
                ref _connectionFailedGridStatus, newStatus);

            if (oldStatus == newStatus)
                return;

            bool changeStyleNeeded;

            if (!TitleLockStatus)
            {
                TitleLockStatus = true;
                _connectionFailedGridTitleLockStatus = true;

                changeStyleNeeded = true;
            }
            else
            {
                changeStyleNeeded = false;
            }

            ConnectionFailedIndicator.IsActive = false;

            if (_connectionFailedGridTitleLockStatus)
                changeStyleNeeded = true;

            var color = ((SolidColorBrush)FindResource("Window.Main.TitleBackground"))
                .Color;

            _connectionFailedGridTask = Task.Run(async () =>
            {
                Action<double> changeOpacityFunction;

                if (changeStyleNeeded)
                {
                    changeOpacityFunction = opacity =>
                    {
                        ConnectionFailedGrid.Opacity = opacity;

                        var brush = new SolidColorBrush(
                            Color.FromArgb((byte)(255 * opacity),
                                color.R, color.G, color.B));

                        WindowTitleBrush = brush;
                        NonActiveWindowTitleBrush = brush;
                    };
                }
                else
                {
                    changeOpacityFunction = opacity =>
                    {
                        ConnectionFailedGrid.Opacity = opacity;
                    };
                }

                var isHitTestVisible = true;

                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (isHitTestVisible
                        && Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ConnectionFailedGrid.IsHitTestVisible = false;

                            Resources.MergedDictionaries
                                .Remove(_loadingStyle.MahAppsDictionary);
                        });

                        isHitTestVisible = false;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        changeOpacityFunction(opacity);
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    if (changeStyleNeeded)
                    {
                        Resources.MergedDictionaries
                            .Remove(_loadingStyle.Dictionary);

                        SetResourceReference(WindowTitleBrushProperty,
                            "Window.Main.TitleBackground");
                        SetResourceReference(NonActiveWindowTitleBrushProperty,
                            "Window.Main.NonActiveTitleBackground");
                    }

                    ConnectionFailedGrid.IsHitTestVisible = false;
                    ConnectionFailedGrid.Visibility = Visibility.Collapsed;

                    ActivateOpenLink();

                    if (changeStyleNeeded)
                    {
                        TitleLockStatus = false;
                        _connectionFailedGridTitleLockStatus = false;
                    }
                });
            });
        }



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);

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



        private async void OnConnectionStateChanged(object sender,
            ConnectionStateChangedEventArgs e)
        {
            await Dispatcher.Invoke(() =>
            {
                return e.NewState switch
                {
                    ConnectionStateType.Connected => HideConnectionFailedGrid(),
                    ConnectionStateType.Disconnected => ShowConnectionFailedGrid(),
                    _ => Task.CompletedTask,
                };
            }).ConfigureAwait(true);
        }



        private void MainWindow_Closed(object sender,
            EventArgs e)
        {
            SettingsManager.AppSettings.WindowWidth = Width;
            SettingsManager.AppSettings.WindowHeight = Height;
            SettingsManager.AppSettings.WindowPositionX = Left + (Width / 2.0);
            SettingsManager.AppSettings.WindowPositionY = Top + (Height / 2.0);
            SettingsManager.AppSettings.WindowState = (int)WindowState;

            SettingsManager.AppSettings.Save();
        }

        private async void OpenLinkButton_Click(object sender,
            RoutedEventArgs e)
        {
            if (!OpenLinkButton.IsEnabled)
                return;

            var title = LocalizationUtils
                .GetLocalized("LinkOpeningTitle");
            var message = LocalizationUtils
                .GetLocalized("EnterURL");

            var link = await DialogManager.ShowSinglelineTextDialog(
                    title, message)
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(link))
                return;



            LinkUtils.OpenLink(link);
        }
    }
}

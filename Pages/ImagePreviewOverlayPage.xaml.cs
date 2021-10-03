using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Memenim.Dialogs;
using Memenim.Downloads;
using Memenim.Pages.ViewModel;
using Memenim.Utils;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class ImagePreviewOverlayPage : PageContent
    {
        public ImagePreviewOverlayViewModel ViewModel
        {
            get
            {
                return DataContext as ImagePreviewOverlayViewModel;
            }
        }

        public ImagePreviewOverlayPage()
        {
            InitializeComponent();
            DataContext = new ImagePreviewOverlayViewModel();
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

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(true);

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            ViewModel.ImageSource = null;

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        private async void CopyImageUrl_Click(object sender, RoutedEventArgs e)
        {
            var imageSource = ViewModel.ImageSource;

            if (imageSource == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(imageSource);
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapSource)img.Source);
        }

        private async void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            await DownloadManager.SaveFile(ViewModel.ImageSource)
                .ConfigureAwait(true);
        }
    }
}

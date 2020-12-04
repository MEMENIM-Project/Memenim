using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Memenim.Downloads;
using Memenim.Pages.ViewModel;
using WpfAnimatedGif;
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

        public async Task LoadImage()
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            ImageBehavior.SetAnimatedSource(img,
                new BitmapImage(new Uri(ViewModel.ImageSource)));

            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(true);

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);
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
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            await LoadImage()
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            ImageBehavior.SetAnimatedSource(img, null);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            base.OnExit(sender, e);
        }

        private void CopyImageUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.ImageSource);
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(new BitmapImage(new Uri(ViewModel.ImageSource)));
        }

        private async void DownloadImage_Click(object sender, RoutedEventArgs e)
        {
            await DownloadManager.SaveFile(ViewModel.ImageSource)
                .ConfigureAwait(true);
        }
    }
}

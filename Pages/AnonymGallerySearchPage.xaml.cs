using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Widgets;
using Memenim.Core.Api;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class AnonymGallerySearchPage : PageContent
    {
        public AnonymGallerySearchViewModel ViewModel
        {
            get
            {
                return DataContext as AnonymGallerySearchViewModel;
            }
        }

        public AnonymGallerySearchPage()
        {
            InitializeComponent();
            DataContext = new AnonymGallerySearchViewModel();
        }

        public async Task ExecuteSearch()
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            var result = await PhotoApi.GetLibraryPhotos()
                .ConfigureAwait(true);

            if (result.IsError
                || result.Data == null)
            {
                return;
            }

            lstImages.Children.Clear();

            foreach (var photo in result.Data)
            {
                if (string.IsNullOrEmpty(photo.MediumUrl))
                    continue;
                if (string.IsNullOrEmpty(photo.SmallUrl))
                    continue;

                ImagePreviewButton previewButton = new ImagePreviewButton()
                {
                    ButtonSize = 200,
                    SmallImageSource = photo.SmallUrl,
                    ImageSource = photo.MediumUrl
                };
                previewButton.Click += ImagePreviewButton_Click;

                lstImages.Children.Add(previewButton);
            }

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
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            await ExecuteSearch()
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            foreach (var button in lstImages.Children)
            {
                ImagePreviewButton imageButton = button as ImagePreviewButton;

                if (imageButton == null)
                    continue;

                imageButton.Image.Source = null;
            }

            lstImages.Children.Clear();

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        private async void ImagePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is ImagePreviewButton target))
                return;

            if (ViewModel.ImageSelectionDelegate != null)
            {
                await ViewModel.ImageSelectionDelegate(
                        target.ImageSource)
                    .ConfigureAwait(true);
            }

            NavigationController.Instance.GoBack();
        }
    }
}

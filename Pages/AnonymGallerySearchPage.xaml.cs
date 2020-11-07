using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Widgets;
using Memenim.Core.Api;

namespace Memenim.Pages
{
    public partial class AnonymGallerySearchPage : PageContent
    {
        public Func<string, Task> OnPicSelect { get; set; }

        public AnonymGallerySearchPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async Task ExecuteSearch()
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            var searchResults = await PhotoApi.GetLibraryPhotos()
                .ConfigureAwait(true);

            if (searchResults.error)
                return;

            lstImages.Children.Clear();

            foreach (var img in searchResults.data)
            {
                if (string.IsNullOrEmpty(img.photo_medium))
                    continue;
                if (string.IsNullOrEmpty(img.photo_small))
                    continue;

                ImagePreviewButton previewButton = new ImagePreviewButton()
                {
                    ButtonSize = 200,
                    ButtonPressAction = OnPicSelect,
                    SmallImageSource = img.photo_small,
                    ImageSource = img.photo_medium
                };

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

                    if (opacity < 0.7)
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

            await ExecuteSearch()
                .ConfigureAwait(true);
        }
    }
}

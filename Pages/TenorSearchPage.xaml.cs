using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Tenor;
using Tenor.Schema;
using Memenim.Commands;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Widgets;
using WpfAnimatedGif;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class TenorSearchPage : PageContent
    {
        private static TenorConfiguration TenorConfig { get; }
        private static TenorClient TenorClient { get; }

        public TenorSearchViewModel ViewModel
        {
            get
            {
                return DataContext as TenorSearchViewModel;
            }
        }

        static TenorSearchPage()
        {
            TenorConfig = new TenorConfiguration
            {
                ApiKey = SettingsManager.PersistentSettings.GetTenorAPIKey(),
                Locale = CultureInfo.CurrentCulture,
                ContentFilter = ContentFilter.Medium,
                MediaFilter = MediaFilter.Minimal,
                AspectRatio = AspectRatio.All
            };

            TenorClient = new TenorClient(TenorConfig);
        }

        public TenorSearchPage()
        {
            InitializeComponent();
            DataContext = new TenorSearchViewModel();
        }

        public async Task ExecuteSearch(string query)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            IEnumerable<ImagePost> searchResults = !string.IsNullOrEmpty(query)
                ? (await TenorClient.SearchAsync(query, 40)
                    .ConfigureAwait(true)).Results
                : (await TenorClient.GetTrendingPostsAsync(40)
                    .ConfigureAwait(true)).Results;

            lstImages.Children.Clear();

            if (searchResults == null)
                return;

            foreach (var data in searchResults)
            {
                ImagePreviewButton previewButton = new ImagePreviewButton
                {
                    ButtonSize = 150,
                    ButtonPressAction = ViewModel.OnPicSelect
                };

                foreach (var media in data.Media)
                {
                    if (!media.TryGetValue(MediaType.TinyGif, out MediaItem value))
                        continue;

                    previewButton.SmallImageSource = value?.Url;

                    if (!media.TryGetValue(MediaType.Gif, out value))
                        continue;

                    previewButton.ImageSource = value?.Url;
                }

                if (previewButton.SmallImageSource == null
                    || previewButton.ImageSource == null)
                {
                    continue;
                }

                lstImages.Children.Add(previewButton);
            }

            await Task.Delay(TimeSpan.FromSeconds(2))
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

            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(true);

            ViewModel.SearchCommand = new AsyncBasicCommand(
                _ => true, async query =>
                {
                    await ExecuteSearch((string)query)
                        .ConfigureAwait(true);
                });

            await ExecuteSearch(txtSearchQuery.Text)
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

                ImageBehavior.SetAnimatedSource(imageButton.img, null);
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
    }
}

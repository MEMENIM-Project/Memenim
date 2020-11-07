using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tenor;
using Tenor.Schema;
using Memenim.Commands;
using Memenim.Settings;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class TennorSearchPage : PageContent
    {
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(TennorSearchPage),
                new PropertyMetadata(new BasicCommand(_ => false)));

        public Func<string, Task> OnPicSelect { get; set; }
        public TenorConfiguration TenorConfig { get; set; }
        public TenorClient TenorClient { get; set; }

        public ICommand SearchCommand
        {
            get
            {
                return (ICommand)GetValue(SearchCommandProperty);
            }
            set
            {
                SetValue(SearchCommandProperty, value);
            }
        }

        public TennorSearchPage()
        {
            InitializeComponent();
            DataContext = this;

            SearchCommand = new BasicCommand(
                _ => true, async query => await ExecuteSearch((string)query)
                    .ConfigureAwait(true)
            );

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

        public async Task ExecuteSearch(string query)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            IEnumerable<ImagePost> searchResults = !string.IsNullOrEmpty(query)
                ? (await TenorClient.SearchAsync(query, 50)
                    .ConfigureAwait(true)).Results
                : (await TenorClient.GetTrendingPostsAsync(50)
                    .ConfigureAwait(true)).Results;

            lstImages.Children.Clear();

            if (searchResults == null)
                return;

            foreach (var data in searchResults)
            {
                ImagePreviewButton previewButton = new ImagePreviewButton
                {
                    ButtonSize = 150,
                    ButtonPressAction = OnPicSelect
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

                //var media = data.Media.First();

                //if (!media.TryGetValue(MediaType.TinyGif, out MediaItem value))
                //    continue;

                //previewButton.SmallImageSource = value?.Url;

                //if (!media.TryGetValue(MediaType.Gif, out value))
                //    continue;

                //previewButton.ImageSource = value?.Url;

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

            await ExecuteSearch(txtSearchQuery.Text)
                .ConfigureAwait(true);
        }
    }
}

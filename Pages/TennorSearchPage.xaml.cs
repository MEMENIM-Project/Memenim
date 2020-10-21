using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Commands;
using Memenim.Widgets;
using Tenor;
using Tenor.Schema;

namespace Memenim.Pages
{
    public partial class TennorSearchPage : UserControl
    {
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(TennorSearchPage),
                new PropertyMetadata(new BasicCommand(_ => false)));

        public Func<string, Task> OnPicSelect { get; set; }

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
        }

        public async Task ExecuteSearch(string query)
        {
            var config = new TenorConfiguration
            {
                ApiKey = AppPersistent.TenorAPIKey,
                Locale = CultureInfo.CurrentCulture,
                ContentFilter = ContentFilter.Medium,
                MediaFilter = MediaFilter.Minimal,
                AspectRatio = AspectRatio.All
            };

            var client = new TenorClient(config);

            IEnumerable<ImagePost> searchResults = !string.IsNullOrEmpty(query)
                ? (await client.SearchAsync(query, 50)
                    .ConfigureAwait(true)).Results
                : (await client.GetTrendingPostsAsync(50)
                    .ConfigureAwait(true)).Results;

            lstImages.Children.Clear();

            if (searchResults == null)
                return;

            foreach (var data in searchResults)
            {
                ImagePreviewButton previewButton = new ImagePreviewButton()
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
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await ExecuteSearch(txtSearchQuery.Text)
                .ConfigureAwait(true);
        }
    }
}

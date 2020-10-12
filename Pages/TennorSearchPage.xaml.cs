using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnonymDesktopClient.Core.Utils;
using AnonymDesktopClient.Core.Widgets;
using Tenor;
using Tenor.Schema;

namespace AnonymDesktopClient.Core.Pages
{
    /// <summary>
    /// Interaction logic for TennorSearchPage.xaml
    /// </summary>
    public partial class TennorSearchPage : UserControl
    {
        public Func<string, Task> OnPicSelect { get; set; }

        public ICommand SearchCommand { get; set; }

        public TennorSearchPage()
        {
            InitializeComponent();
            SearchCommand = new BasicCommand(
                o => true,
                async q => { await ExecuteSearch((string)q); }
            );

            DataContext = this;
        }

        public async Task ExecuteSearch(string query)
        {
            var config = new TenorConfiguration
            {
                ApiKey = AppPersistent.TenorAPIKey,
                Locale = CultureInfo.GetCultureInfo("en"),
                ContentFilter = ContentFilter.Medium,
                MediaFilter = MediaFilter.Minimal,
                AspectRatio = AspectRatio.All
            };
            var client = new TenorClient(config);
            var searchResults = await client.SearchAsync(query, limit: 50);

            lslImages.Children.Clear();

            foreach (var data in searchResults.Results)
            {
                ImagePreviewButton previewButton = new ImagePreviewButton()
                {
                    ButtonSize = 150,
                    ButtonPressAction = OnPicSelect
                };
                MediaItem value;
                foreach (var media in data.Media)
                {
                    media.TryGetValue(MediaType.TinyGif, out value);
                    previewButton.PreviewImage = value.Url;
                    media.TryGetValue(MediaType.Gif, out value);
                    previewButton.ValueImage = value.Url;
                }

                lslImages.Children.Add(previewButton);
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteSearch(txtSearchQuery.Text);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Tenor;
using Tenor.Schema;
using Memenim.Commands;
using Memenim.Navigation;
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
                ApiKey = SettingsManager.PersistentSettings
                    .GetTenorAPIKey(),
                Locale = CultureInfo.CurrentCulture,
                ContentFilter = ContentFilter.Medium,
                MediaFilter = MediaFilter.Minimal,
                AspectRatio = AspectRatio.All
            };
            TenorClient = new TenorClient(
                TenorConfig);
        }

        public TenorSearchPage()
        {
            InitializeComponent();
            DataContext = new TenorSearchViewModel();
        }



        public async Task ExecuteSearch(string query)
        {
            await ShowLoadingGrid()
                .ConfigureAwait(true);

            var searchResults = !string.IsNullOrEmpty(query)
                ? (await TenorClient.SearchAsync(query, 40)
                    .ConfigureAwait(true)).Results
                : (await TenorClient.GetTrendingPostsAsync(40)
                    .ConfigureAwait(true)).Results;

            ImagesWrapPanel.Children.Clear();

            if (searchResults == null)
                return;

            foreach (var data in searchResults)
            {
                var imageButton = new ImagePreviewButton
                {
                    ButtonSize = 150
                };
                imageButton.Click += ImagePreviewButton_Click;

                foreach (var media in data.Media)
                {
                    if (!media.TryGetValue(MediaType.Gif, out var value))
                        continue;

                    imageButton.ImageSource = value?.Url;

                    if (!media.TryGetValue(MediaType.TinyGif, out value))
                        value = media[MediaType.Gif];

                    imageButton.SmallImageSource = value?.Url;
                }

                if (imageButton.ImageSource == null
                    || imageButton.SmallImageSource == null)
                {
                    continue;
                }

                ImagesWrapPanel.Children.Add(
                    imageButton);
            }

            await Task.Delay(
                    TimeSpan.FromSeconds(2))
                .ConfigureAwait(true);

            await HideLoadingGrid()
                .ConfigureAwait(true);
        }



        public Task ShowLoadingGrid()
        {
            LoadingIndicator.IsActive = true;
            LoadingGrid.Opacity = 1.0;
            LoadingGrid.IsHitTestVisible = true;
            LoadingGrid.Visibility = Visibility.Visible;

            return Task.CompletedTask;
        }

        public Task HideLoadingGrid()
        {
            LoadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (var i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LoadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    LoadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }



        protected override async void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            ViewModel.SearchCommand = new AsyncBasicCommand(
                async query =>
                {
                    await ExecuteSearch(
                            (string)query)
                        .ConfigureAwait(true);
                });

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await ShowLoadingGrid()
                .ConfigureAwait(true);

            await Task.Delay(TimeSpan.FromSeconds(1))
                .ConfigureAwait(true);

            await ExecuteSearch(
                    SearchQueryTextBox.Text)
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            foreach (var button in ImagesWrapPanel.Children)
            {
                if (!(button is ImagePreviewButton imageButton))
                    continue;

                ImageBehavior.SetAnimatedSource(
                    imageButton.Image, null);
            }

            ImagesWrapPanel.Children.Clear();

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private async void ImagePreviewButton_Click(object sender,
            RoutedEventArgs e)
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

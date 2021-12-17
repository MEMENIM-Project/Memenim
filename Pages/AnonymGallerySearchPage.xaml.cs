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
            await ShowLoadingGrid()
                .ConfigureAwait(true);

            try
            {
                var result = await PhotoApi
                    .GetLibraryPhotos()
                    .ConfigureAwait(true);

                if (result.IsError
                    || result.Data == null)
                {
                    return;
                }

                ImagesWrapPanel.Children.Clear();

                foreach (var photo in result.Data)
                {
                    if (string.IsNullOrEmpty(photo.MediumUrl))
                        continue;
                    if (string.IsNullOrEmpty(photo.SmallUrl))
                        photo.SmallUrl = photo.MediumUrl;

                    var imageButton = new ImagePreviewButton
                    {
                        ButtonSize = 200,
                        SmallImageSource = photo.SmallUrl,
                        ImageSource = photo.MediumUrl,
                        Margin = new Thickness(5)
                    };
                    imageButton.Click += ImagePreviewButton_Click;

                    ImagesWrapPanel.Children.Add(
                        imageButton);
                }

                await Task.Delay(
                        TimeSpan.FromSeconds(1))
                    .ConfigureAwait(true);
            }
            finally
            {
                await HideLoadingGrid()
                    .ConfigureAwait(true);
            }
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

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await ShowLoadingGrid()
                .ConfigureAwait(true);

            await ExecuteSearch()
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

                imageButton.Image.Source =
                    null;
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

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Widgets;
using Memenim.Core.Api;

namespace Memenim.Pages
{
    public partial class AnonymGallerySearchPage : UserControl
    {
        public Func<string, Task> OnPicSelect { get; set; }

        public AnonymGallerySearchPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async Task ExecuteSearch()
        {
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
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await ExecuteSearch()
                .ConfigureAwait(true);
        }
    }
}

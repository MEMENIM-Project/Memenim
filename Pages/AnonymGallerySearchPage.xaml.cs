using AnonymDesktopClient.Widgets;
using Memenim.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for AnonymGallerySearchPage.xaml
    /// </summary>
    public partial class AnonymGallerySearchPage : UserControl
    {
        public Func<string, Task> OnPicSelect { get; set; }

        public AnonymGallerySearchPage()
        {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await ExecuteSearch();
        }

        public async Task ExecuteSearch()
        {
            var searchResults = await PhotoAPI.GetLibraryPhotos();

            if(searchResults.error)
            {
                return;
            }

            lslImages.Children.Clear();

            foreach (var img in searchResults.data)
            {
                ImagePreviewButton previewButton = new ImagePreviewButton()
                {
                    ButtonSize = 200,
                    ButtonPressAction = OnPicSelect,
                    ValueImage = img.photo_medium,
                    PreviewImage = img.photo_medium
                };
                lslImages.Children.Add(previewButton);
            }

        }
    }
}

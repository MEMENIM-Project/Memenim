using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Memenim.Pages.ViewModel;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for ImageViewPage.xaml
    /// </summary>
    public partial class ImagePreviewPage : PageContent
    {
        public ImagePreviewViewModel ViewModel
        {
            get
            {
                return DataContext as ImagePreviewViewModel;
            }
        }

        public ImagePreviewPage()
        {
            InitializeComponent();
            DataContext = new ImagePreviewViewModel();
        }

        private void ClosePreview_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigationController.Instance.GoBack();
        }
    }
}

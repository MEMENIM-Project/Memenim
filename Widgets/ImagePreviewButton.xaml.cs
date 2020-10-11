using AnonymDesktopClient.Pages;
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

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for ImagePreviewButton.xaml
    /// </summary>
    public partial class ImagePreviewButton : UserControl
    {
        public string PreviewImage { get; set; }
        public string ValueImage { get; set; }
        public int ButtonSize { get; set; }

        public Func<string, Task> ButtonPressAction;

        public ImagePreviewButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            await ButtonPressAction(ValueImage);
            PageNavigationManager.GoBack();
        }
    }
}

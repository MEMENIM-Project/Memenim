using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Pages;
using Memenim.Managers;

namespace Memenim.Widgets
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

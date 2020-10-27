using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Managers;
using Memenim.Pages;

namespace Memenim.Widgets
{
    public partial class ImagePreviewButton : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ImagePreviewButton), new PropertyMetadata((string)null));
        public static readonly DependencyProperty SmallImageSourceProperty =
            DependencyProperty.Register("SmallImageSource", typeof(string), typeof(ImagePreviewButton), new PropertyMetadata((string)null));

        public Func<string, Task> ButtonPressAction;

        public string ImageSource
        {
            get
            {
                return (string)GetValue(ImageSourceProperty);
            }
            set
            {
                SetValue(ImageSourceProperty, value);
            }
        }
        public string SmallImageSource
        {
            get
            {
                return (string)GetValue(SmallImageSourceProperty);
            }
            set
            {
                SetValue(SmallImageSourceProperty, value);
            }
        }
        public int ButtonSize { get; set; }

        public ImagePreviewButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            await ButtonPressAction(ImageSource)
                .ConfigureAwait(true);

            //PageStorage.GoBack();
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
{
    public partial class ImagePreviewButton : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ImagePreviewButton), new PropertyMetadata((string)null));
        public static readonly DependencyProperty SmallImageSourceProperty =
            DependencyProperty.Register("SmallImageSource", typeof(string), typeof(ImagePreviewButton), new PropertyMetadata((string)null));
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register("ButtonSize", typeof(int), typeof(ImagePreviewButton), new PropertyMetadata(100));

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
        public int ButtonSize
        {
            get
            {
                return (int)GetValue(ButtonSizeProperty);
            }
            set
            {
                SetValue(ButtonSizeProperty, value);
            }
        }

        public ImagePreviewButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Preview_Click(object sender, RoutedEventArgs e)
        {
            await ButtonPressAction(ImageSource)
                .ConfigureAwait(true);

            //NavigationController.Instance.GoBack();
        }
    }
}

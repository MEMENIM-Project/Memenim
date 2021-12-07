using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Navigation;

namespace Memenim.Widgets
{
    public partial class ImagePreviewButton : WidgetContent
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(string), typeof(ImagePreviewButton),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty SmallImageSourceProperty =
            DependencyProperty.Register(nameof(SmallImageSource), typeof(string), typeof(ImagePreviewButton),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(nameof(ButtonSize), typeof(double), typeof(ImagePreviewButton),
                new PropertyMetadata(100D));



        private Func<string, Task> _clickFunction;
        public Func<string, Task> ClickFunction
        {
            get
            {
                return _clickFunction;
            }
            set
            {
                _clickFunction = value;
                OnPropertyChanged(nameof(ClickFunction));
            }
        }



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
        public double ButtonSize
        {
            get
            {
                return (double)GetValue(ButtonSizeProperty);
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



        private async void Button_Click(object sender,
            RoutedEventArgs e)
        {
            if (ClickFunction != null)
            {
                await ClickFunction(ImageSource)
                    .ConfigureAwait(true);
            }

            NavigationController.Instance.GoBack();
        }
    }
}

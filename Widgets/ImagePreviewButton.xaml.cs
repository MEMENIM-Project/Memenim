using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Navigation;

namespace Memenim.Widgets
{
    public partial class ImagePreviewButton : WidgetContent
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(ImagePreviewButton));



        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(string), typeof(ImagePreviewButton),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty SmallImageSourceProperty =
            DependencyProperty.Register(nameof(SmallImageSource), typeof(string), typeof(ImagePreviewButton),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(nameof(ButtonSize), typeof(double), typeof(ImagePreviewButton),
                new PropertyMetadata(100D));



        public event EventHandler<RoutedEventArgs> Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
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



        private void Button_Click(object sender,
            RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
}

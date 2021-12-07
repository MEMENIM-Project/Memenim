using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class IconButton : WidgetContent
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(IconButton));
        


        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(IconButton),
                new PropertyMetadata(16D));
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(IconButton),
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(Enum), typeof(IconButton),
                new PropertyMetadata((Enum)PackIconModernKind.Xbox));
        public static readonly DependencyProperty InformationProperty =
            DependencyProperty.Register(nameof(Information), typeof(string), typeof(IconButton),
                new PropertyMetadata(string.Empty));



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



        public double IconSize
        {
            get
            {
                return (double)GetValue(IconSizeProperty);
            }
            set
            {
                SetValue(IconSizeProperty, value);
            }
        }
        public Brush IconForeground
        {
            get
            {
                return (Brush)GetValue(IconForegroundProperty);
            }
            set
            {
                SetValue(IconForegroundProperty, value);
            }
        }
        public Enum IconKind
        {
            get
            {
                return (Enum)GetValue(IconKindProperty);
            }
            set
            {
                SetValue(IconKindProperty, value);
            }
        }
        public string Information
        {
            get
            {
                return (string)GetValue(InformationProperty);
            }
            set
            {
                SetValue(InformationProperty, value);
            }
        }



        public IconButton()
        {
            InitializeComponent();
            DataContext = this;

            SetResourceReference(IconForegroundProperty,
                "MahApps.Brushes.IdealForeground");
        }



        private void Button_Click(object sender,
            RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
}

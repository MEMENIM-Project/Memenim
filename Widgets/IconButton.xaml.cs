using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class IconButton : UserControl
    {
        public static readonly RoutedEvent OnIconButtonClicked =
            EventManager.RegisterRoutedEvent(nameof(IconButtonClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(IconButton));
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(int), typeof(IconButton),
                new PropertyMetadata(16));
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(IconButton),
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(PackIconModernKind), typeof(IconButton),
                new PropertyMetadata(PackIconModernKind.Xbox));
        public static readonly DependencyProperty InformationProperty =
            DependencyProperty.Register(nameof(Information), typeof(string), typeof(IconButton),
                new PropertyMetadata(string.Empty));

        public event EventHandler<RoutedEventArgs> IconButtonClick
        {
            add
            {
                AddHandler(OnIconButtonClicked, value);
            }
            remove
            {
                RemoveHandler(OnIconButtonClicked, value);
            }
        }

        public int IconSize
        {
            get
            {
                return (int)GetValue(IconSizeProperty);
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
        public PackIconModernKind IconKind
        {
            get
            {
                return (PackIconModernKind)GetValue(IconKindProperty);
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

            SetResourceReference(IconForegroundProperty, "MahApps.Brushes.IdealForeground");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnIconButtonClicked));
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class StatButton : WidgetContent
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(StatButton));



        public static readonly DependencyProperty StatValueProperty =
            DependencyProperty.Register(nameof(StatValue), typeof(string), typeof(StatButton),
                new PropertyMetadata("0"));
        public static readonly DependencyProperty StatValueOnLeftProperty =
            DependencyProperty.Register(nameof(StatValueOnLeft), typeof(bool), typeof(StatButton),
                new PropertyMetadata(false));
        public static readonly DependencyProperty StatValueFontSizeProperty =
            DependencyProperty.Register(nameof(StatValueFontSize), typeof(double), typeof(StatButton),
                new PropertyMetadata((double) TextBlock.FontSizeProperty.DefaultMetadata.DefaultValue));
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(nameof(ButtonSize), typeof(double), typeof(StatButton),
                new PropertyMetadata(48D));
        public static readonly DependencyProperty BorderSizeProperty =
            DependencyProperty.Register(nameof(BorderSize), typeof(double), typeof(StatButton),
                new PropertyMetadata(1.5));
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(StatButton),
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty BorderBackgroundProperty =
            DependencyProperty.Register(nameof(BorderBackground), typeof(Brush), typeof(StatButton),
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(StatButton),
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(Enum), typeof(StatButton),
                new PropertyMetadata((Enum)PackIconModernKind.Xbox));



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



        public string StatValue
        {
            get
            {
                return (string)GetValue(StatValueProperty);
            }
            set
            {
                SetValue(StatValueProperty, value);
            }
        }
        public bool StatValueOnLeft
        {
            get
            {
                return (bool)GetValue(StatValueOnLeftProperty);
            }
            set
            {
                SetValue(StatValueOnLeftProperty, value);
            }
        }
        public double StatValueFontSize
        {
            get
            {
                return (double)GetValue(StatValueFontSizeProperty);
            }
            set
            {
                SetValue(StatValueFontSizeProperty, value);
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
        public double BorderSize
        {
            get
            {
                return (double)GetValue(BorderSizeProperty);
            }
            set
            {
                SetValue(BorderSizeProperty, value);
            }
        }
        public Brush ButtonBackground
        {
            get
            {
                return (Brush)GetValue(ButtonBackgroundProperty);
            }
            set
            {
                SetValue(ButtonBackgroundProperty, value);
            }
        }
        public Brush BorderBackground
        {
            get
            {
                return (Brush)GetValue(BorderBackgroundProperty);
            }
            set
            {
                SetValue(BorderBackgroundProperty, value);
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



        public StatButton()
        {
            InitializeComponent();
            DataContext = this;

            SetResourceReference(BorderBackgroundProperty, "MahApps.Brushes.Gray3");
            SetResourceReference(IconForegroundProperty, "MahApps.Brushes.Accent");
        }



        private void Button_Click(object sender,
            RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
}

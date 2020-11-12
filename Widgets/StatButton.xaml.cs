using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class StatButton : UserControl
    {
        public static readonly RoutedEvent OnButtonClicked =
            EventManager.RegisterRoutedEvent(nameof(ButtonClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(StatButton));
        public static readonly DependencyProperty StatValueProperty =
            DependencyProperty.Register(nameof(StatValue), typeof(string), typeof(StatButton),
                new PropertyMetadata("0"));
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(nameof(ButtonSize), typeof(int), typeof(StatButton),
                new PropertyMetadata(48));
        public static readonly DependencyProperty BorderSizeProperty =
            DependencyProperty.Register(nameof(BorderSize), typeof(double), typeof(StatButton),
                new PropertyMetadata(1.5));
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(StatButton),
                new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty BorderBackgroundProperty =
            DependencyProperty.Register(nameof(BorderBackground), typeof(Brush), typeof(StatButton),
                new PropertyMetadata(Brushes.Transparent));

        public event EventHandler<RoutedEventArgs> ButtonClick
        {
            add
            {
                AddHandler(OnButtonClicked, value);
            }
            remove
            {
                RemoveHandler(OnButtonClicked, value);
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
        public PackIconModernKind IconKind { get; set; } = PackIconModernKind.Xbox;

        public StatButton()
        {
            InitializeComponent();
            DataContext = this;

            SetResourceReference(BorderBackgroundProperty, "MahApps.Brushes.Gray3");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnButtonClicked));
        }
    }
}

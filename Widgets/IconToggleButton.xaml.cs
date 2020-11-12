using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class IconToggleButton : UserControl
    {
        public static readonly RoutedEvent OnIconToggleButtonClicked =
            EventManager.RegisterRoutedEvent(nameof(IconToggleButtonClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(IconToggleButton));
        public static readonly DependencyProperty PageNameProperty =
            DependencyProperty.Register(nameof(PageName), typeof(string), typeof(IconToggleButton),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(PackIconModernKind), typeof(IconToggleButton),
                new PropertyMetadata(PackIconModernKind.Xbox));

        public event EventHandler<RoutedEventArgs> IconToggleButtonClick
        {
            add
            {
                AddHandler(OnIconToggleButtonClicked, value);
            }
            remove
            {
                RemoveHandler(OnIconToggleButtonClicked, value);
            }
        }

        public string PageName
        {
            get
            {
                return (string)GetValue(PageNameProperty);
            }
            set
            {
                SetValue(PageNameProperty, value);
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

        public IconToggleButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnIconToggleButtonClicked));
        }
    }
}

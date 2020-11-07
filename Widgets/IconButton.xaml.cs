using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class IconButton : UserControl
    {
        public static readonly RoutedEvent OnIconButtonClicked =
            EventManager.RegisterRoutedEvent("OnIconButtonClick", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(IconButton));
        public static readonly DependencyProperty PageNameProperty =
            DependencyProperty.Register(nameof(PageName), typeof(string), typeof(IconButton),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(nameof(IconKind), typeof(PackIconModernKind), typeof(IconButton),
                new PropertyMetadata(PackIconModernKind.Xbox));

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

        public IconButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnIconButtonClicked));
        }
    }
}

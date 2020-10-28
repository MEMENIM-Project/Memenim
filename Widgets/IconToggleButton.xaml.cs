using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class IconToggleButton : UserControl
    {
        public static readonly RoutedEvent OnIconToggleButtonClicked = EventManager.RegisterRoutedEvent("OnIconToggleButtonClick", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(IconToggleButton));

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

        public string PageName { get; set; }
        public PackIconModernKind IconKind { get; set; } = PackIconModernKind.Xbox;

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

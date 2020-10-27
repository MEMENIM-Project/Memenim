using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class IconButton : UserControl
    {
        public static readonly RoutedEvent OnIconButtonClicked = EventManager.RegisterRoutedEvent("OnIconButtonClick", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(IconButton));

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

        public PackIconModernKind IconKind { get; set; }
        public string PageName { get; set; }

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

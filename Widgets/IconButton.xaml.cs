using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace AnonymDesktopClient.Core.Widgets
{
    /// <summary>
    /// Interaction logic for IconButton.xaml
    /// </summary>
    public partial class IconButton : UserControl
    {

        public PackIconModernKind IconKind { get; set; }
        public string ImageSource { get; set; } = "";
        public string RedirectTag { get; set; } = "";


        public static readonly RoutedEvent OnIconButtonClicked = EventManager.RegisterRoutedEvent("OnIconButtonClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(IconButton));

        // expose our event
        public event RoutedEventHandler IconButtonClick
        {
            add { AddHandler(OnIconButtonClicked, value); }
            remove { RemoveHandler(OnIconButtonClicked, value); }
        }

        public IconButton()
        {
            InitializeComponent();
            DataContext = this;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OnIconButtonClicked);
            RaiseEvent(newEventArgs);
        }
    }
}

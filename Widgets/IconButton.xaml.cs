using ControlzEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MahApps.Metro.IconPacks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnonymDesktopClient.Widgets
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

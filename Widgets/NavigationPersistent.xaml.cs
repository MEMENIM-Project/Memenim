using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for NavigationPersistent.xaml
    /// </summary>
    public partial class NavigationPersistent : UserControl
    {
        public static readonly RoutedEvent RedirectEvent;

        static NavigationPersistent()
        {
            RedirectEvent = EventManager.RegisterRoutedEvent("RedirectRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationPersistent));
        }

        public NavigationPersistent()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler RedirectRequested
        {
            add { AddHandler(NavigationPersistent.RedirectEvent, value); }
            remove { RemoveHandler(NavigationPersistent.RedirectEvent, value); }
        }

        void RaiseRedirectEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(NavigationPersistent.RedirectEvent);
            RaiseEvent(newEventArgs);
        }


        private void btnNavHome_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EPageToRedirect, "AllPosts");
            RaiseRedirectEvent();
        }

        private void btnNavSettings_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EPageToRedirect, "Settings");
            RaiseRedirectEvent();
        }
    }
}

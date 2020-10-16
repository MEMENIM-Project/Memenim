using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
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


        private void OnNavButtonClick(object sender, RoutedEventArgs e)
        {
            IconButton btn = sender as IconButton;

            GeneralBlackboard.SetValue(BlackBoardValues.EPageToRedirect, btn.RedirectTag);
            RaiseRedirectEvent();
        }


    }
}

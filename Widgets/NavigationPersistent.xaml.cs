using MahApps.Metro.Controls;
using Memenim.Managers;
using Memenim.Pages;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for NavigationPersistent.xaml
    /// </summary>
    public partial class TabNavigationController : UserControl
    {
        public static readonly RoutedEvent RedirectEvent;

        public object PageContent { get; set; }


        static TabNavigationController()
        {
            RedirectEvent = EventManager.RegisterRoutedEvent("RedirectRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TabNavigationController));
        }

        public TabNavigationController()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler RedirectRequested
        {
            add { AddHandler(TabNavigationController.RedirectEvent, value); }
            remove { RemoveHandler(TabNavigationController.RedirectEvent, value); }
        }

        void RaiseRedirectEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(TabNavigationController.RedirectEvent);
            RaiseEvent(newEventArgs);
        }


        private void OnNavButtonClick(object sender, RoutedEventArgs e)
        {
            IconButton btn = sender as IconButton;

            GeneralBlackboard.SetValue(BlackBoardValues.EPageToRedirect, btn.RedirectTag);

            //PageContent = PageNavigationManager.GetPage<FeedPage>();

            RaiseRedirectEvent();
        }


    }
}

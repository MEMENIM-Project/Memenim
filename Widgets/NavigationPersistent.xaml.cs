using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
{
    public partial class NavigationPersistent : UserControl
    {
        public static readonly RoutedEvent RedirectEvent = EventManager.RegisterRoutedEvent("RedirectRequested", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(NavigationPersistent));

        public event EventHandler<RoutedEventArgs> RedirectRequested
        {
            add
            {
                AddHandler(NavigationPersistent.RedirectEvent, value);
            }
            remove
            {
                RemoveHandler(NavigationPersistent.RedirectEvent, value);
            }
        }

        public NavigationPersistent()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void RaiseRedirectEvent()
        {
            RaiseEvent(new RoutedEventArgs(RedirectEvent));
        }

        private void OnNavButtonClick(object sender, RoutedEventArgs e)
        {
            IconButton button = sender as IconButton;

            GeneralBlackboard.SetValue(BlackBoardValues.EPageToRedirect, button?.RedirectTag);
            RaiseRedirectEvent();
        }
    }
}

using MahApps.Metro.Controls;
using Memenim.Managers;
using Memenim.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for NavigationPersistent.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        public static readonly RoutedEvent RedirectEvent;

        public object PageContent { get; set; }


        static NavigationBar()
        {
            RedirectEvent = EventManager.RegisterRoutedEvent("RedirectRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationBar));
        }

        public NavigationBar()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler RedirectRequested
        {
            add { AddHandler(NavigationBar.RedirectEvent, value); }
            remove { RemoveHandler(NavigationBar.RedirectEvent, value); }
        }

        void RaiseRedirectEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(NavigationBar.RedirectEvent);
            RaiseEvent(newEventArgs);
        }


        private async void OnNavButtonClick(object sender, RoutedEventArgs e)
        {
            IconButton btn = sender as IconButton;

            try
            {
                Type t = Type.GetType("Memenim.Pages." + btn.PageName);
                NavigationController.Instance.RequestPage(t);
            }
            catch(TypeLoadException ex)
            {
                await DialogManager.ShowDialog("Error", ex.Message);
            }

            RaiseRedirectEvent();
        }


    }
}

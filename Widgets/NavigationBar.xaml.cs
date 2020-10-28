using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Dialogs;
using Memenim.Navigation;

namespace Memenim.Widgets
{
    public partial class NavigationBar : UserControl
    {
        public static readonly RoutedEvent RedirectEvent =
            EventManager.RegisterRoutedEvent("RedirectRequested", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(NavigationBar));

        public event EventHandler<RoutedEventArgs> RedirectRequested
        {
            add
            {
                AddHandler(RedirectEvent, value);
            }
            remove
            {
                RemoveHandler(RedirectEvent, value);
            }
        }

        public object PageContent { get; set; }

        public NavigationBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void OnNavButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is IconButton button)
                {
                    if (button.PageName == "Back")
                        NavigationController.Instance.GoBack();
                    else
                        NavigationController.Instance.RequestPage(Type.GetType("Memenim.Pages." + button.PageName));
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Navigation error", ex.Message)
                    .ConfigureAwait(true);
            }

            RaiseEvent(new RoutedEventArgs(RedirectEvent));
        }
    }
}

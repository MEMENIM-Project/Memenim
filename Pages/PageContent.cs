using System;
using System.Windows.Controls;

namespace Memenim.Pages
{
    public class PageContent : UserControl
    {
        public PageContent()
        {
            Loaded += OnEnter;
            Unloaded += OnExit;
        }

        protected virtual void OnEnter(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        protected virtual void OnExit(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}

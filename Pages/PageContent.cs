using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Memenim.Pages
{
    public partial class PageContent : UserControl
    {
        public PageContent()
        {
            Loaded += OnEnter;
            Unloaded += OnExit;
        }

        protected virtual void OnEnter(object sender, System.Windows.RoutedEventArgs e) { }

        protected virtual void OnExit(object sender, System.Windows.RoutedEventArgs e) { }
    }
}

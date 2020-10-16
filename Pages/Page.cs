using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AnonymDesktopClient.Core.Pages
{
    public partial class Page : UserControl
    {
        public Page()
        {
            Loaded += OnEnter;
            Unloaded += OnExit;
        }

        protected virtual void OnEnter(object sender, System.Windows.RoutedEventArgs e) { }

        protected virtual void OnExit(object sender, System.Windows.RoutedEventArgs e) { }
    }
}

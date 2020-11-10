using System;
using System.Windows.Controls;

namespace Memenim.Pages
{
    public abstract class PageContent : UserControl
    {
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }

        protected PageContent()
        {
            Loaded += OnEnter;
            Unloaded += OnExit;

            IsOnEnterActive = true;
            IsOnExitActive = true;
        }

        protected virtual void OnEnter(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnExit(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }
    }
}

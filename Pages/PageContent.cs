using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Memenim.Pages.ViewModel;

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
            DataContextChanged += OnDataContextChanged;

            IsOnEnterActive = true;
            IsOnExitActive = true;
        }

        protected virtual void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnExit(object sender, RoutedEventArgs e)
        {
            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.OldValue != null)
                    ((PageViewModel) e.OldValue).PropertyChanged -= ViewModelPropertyChanged;
            }
            catch (NullReferenceException)
            {

            }

            if (e.NewValue != null)
            {
                ((PageViewModel) e.NewValue).PropertyChanged += ViewModelPropertyChanged;

                if (!IsOnEnterActive)
                    ((PageViewModel) e.NewValue).OnPropertyChanged(string.Empty);
            }
        }

        protected virtual void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }
    }
}

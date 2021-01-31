using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Memenim.Pages.ViewModel;

namespace Memenim.Pages
{
    public abstract class PageContent : UserControl
    {
        public bool IsCreated { get; private set; }
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }
        public PageStateType PageState { get; set; }

        protected PageContent()
        {
            Initialized += OnCreated;
            Initialized += OnInitialized;
            Loaded += OnEnter;
            Unloaded += OnExit;
            DataContextChanged += OnDataContextChanged;

            IsCreated = false;
            IsOnEnterActive = true;
            IsOnExitActive = true;
            PageState = PageStateType.Unknown;
        }

        protected virtual void OnCreated(object sender, EventArgs e)
        {
            if (IsCreated)
                return;

            Initialized -= OnCreated;

            IsCreated = true;
        }

        protected virtual void OnInitialized(object sender, EventArgs e)
        {

        }

        protected virtual void OnEnter(object sender, RoutedEventArgs e)
        {
            PageState = PageStateType.Loaded;

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnExit(object sender, RoutedEventArgs e)
        {
            PageState = PageStateType.Unloaded;

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

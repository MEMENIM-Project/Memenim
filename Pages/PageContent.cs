using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Memenim.Generic;
using Memenim.Pages.ViewModel;

namespace Memenim.Pages
{
    public abstract class PageContent : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        public bool IsCreated { get; private set; }
        public bool IsFirstEntered { get; private set; }
        public bool IsFirstExited { get; private set; }
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }
        public ControlStateType State { get; private set; }



        protected PageContent()
        {
            Initialized += OnCreated;
            Initialized += OnInitialized;
            Loaded += OnFirstEnter;
            Loaded += OnEnter;
            Unloaded += OnFirstExit;
            Unloaded += OnExit;
            DataContextChanged += OnDataContextChanged;

            IsCreated = false;
            IsFirstEntered = false;
            IsFirstExited = false;
            IsOnEnterActive = true;
            IsOnExitActive = true;
            State = ControlStateType.Unknown;
        }



        protected virtual void OnCreated(object sender,
            EventArgs e)
        {
            if (IsCreated)
                return;

            Initialized -= OnCreated;

            IsCreated = true;
        }

        protected virtual void OnInitialized(object sender,
            EventArgs e)
        {

        }

        protected virtual void OnFirstEnter(object sender,
            RoutedEventArgs e)
        {
            if (IsFirstEntered)
                return;

            Loaded -= OnFirstEnter;

            IsFirstEntered = true;
        }

        protected virtual void OnEnter(object sender,
            RoutedEventArgs e)
        {
            State = ControlStateType.Loaded;

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnFirstExit(object sender,
            RoutedEventArgs e)
        {
            if (IsFirstExited)
                return;

            Unloaded -= OnFirstExit;

            IsFirstExited = true;
        }

        protected virtual void OnExit(object sender,
            RoutedEventArgs e)
        {
            State = ControlStateType.Unloaded;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnDataContextChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.OldValue != null)
                    ((PageViewModel)e.OldValue).PropertyChanged -= ViewModelPropertyChanged;
            }
            catch (NullReferenceException)
            {

            }

            if (e.NewValue != null)
            {
                ((PageViewModel)e.NewValue).PropertyChanged += ViewModelPropertyChanged;

                ((PageViewModel)e.NewValue).OnPropertyChanged(string.Empty);
            }
        }

        protected virtual void ViewModelPropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {

        }



        public virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}

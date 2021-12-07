using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Memenim.Generic;

namespace Memenim.Widgets
{
    public abstract class WidgetContent : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        public bool IsCreated { get; private set; }
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }
        public ControlStateType State { get; private set; }



        protected WidgetContent()
        {
            Initialized += OnCreated;
            Initialized += OnInitialized;
            Loaded += OnEnter;
            Unloaded += OnExit;
            DataContextChanged += OnDataContextChanged;

            IsCreated = false;
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

        }



        public virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}

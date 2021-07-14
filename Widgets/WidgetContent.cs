using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
{
    public abstract class WidgetContent : UserControl
    {
        public bool IsCreated { get; private set; }
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }
        public WidgetStateType State { get; protected set; }

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
            State = WidgetStateType.Unknown;
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
            State = WidgetStateType.Loaded;

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnExit(object sender, RoutedEventArgs e)
        {
            State = WidgetStateType.Unloaded;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}

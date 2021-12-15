using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Memenim.Generic;

namespace Memenim.SpecialEvents.Layers
{
    public abstract class SpecialEventLayerContent : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;



        public bool IsCreated { get; private set; }
        public bool IsFirstEntered { get; private set; }
        public bool IsFirstExited { get; private set; }
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }
        public ControlStateType State { get; private set; }



        public abstract string DefaultMusicDirectoryPath { get; }
        public abstract string CustomMusicDirectoryPath { get; }
        public abstract bool EventEnabled { get; protected set; }
        public abstract bool EventLoaded { get; protected set; }



        protected SpecialEventLayerContent()
        {
            Initialized += OnCreated;
            Initialized += OnInitialized;
            Loaded += OnFirstEnter;
            Loaded += OnEnter;
            Unloaded += OnFirstExit;
            Unloaded += OnExit;

            IsCreated = false;
            IsFirstEntered = false;
            IsFirstExited = false;
            IsOnEnterActive = true;
            IsOnExitActive = true;
            State = ControlStateType.Unknown;
        }



        public abstract bool EventTimeSatisfied(
            DateTime currentTime);

        public abstract bool LoadEvent();

        public abstract void UnloadEvent();

        public abstract void Activate();

        public abstract void Deactivate();

        public abstract void SetVolume(
            double value);



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



        public virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}

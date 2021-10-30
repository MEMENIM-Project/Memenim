using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.SpecialEvents.Layers
{
    public abstract class SpecialEventLayerContent : UserControl
    {
        public bool IsCreated { get; private set; }
        public bool IsOnEnterActive { get; set; }
        public bool IsOnExitActive { get; set; }
        public SpecialEventLayerStateType State { get; set; }

        public abstract string DefaultMusicDirectoryPath { get; }
        public abstract string CustomMusicDirectoryPath { get; }
        public abstract bool EventEnabled { get; protected set; }
        public abstract bool EventLoaded { get; protected set; }



        protected SpecialEventLayerContent()
        {
            Initialized += OnCreated;
            Initialized += OnInitialized;
            Loaded += OnEnter;
            Unloaded += OnExit;

            IsCreated = false;
            IsOnEnterActive = true;
            IsOnExitActive = true;
            State = SpecialEventLayerStateType.Unknown;
        }



        internal abstract bool EventTimeSatisfied(
            DateTime currentTime);

        internal abstract bool LoadEvent();

        internal abstract void UnloadEvent();

        public abstract void Activate(
            bool state);

        public abstract void SetVolume(
            double value);



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
            State = SpecialEventLayerStateType.Loaded;

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected virtual void OnExit(object sender, RoutedEventArgs e)
        {
            State = SpecialEventLayerStateType.Unloaded;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }
    }
}

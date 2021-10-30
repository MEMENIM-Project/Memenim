using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows;
using Memenim.Settings;
using Memenim.SpecialEvents.Layers;
using Memenim.Utils;
using RIS.Logging;

namespace Memenim.SpecialEvents
{
    public static class SpecialEventManager
    {
        public static event EventHandler<EventArgs> EventUpdated;



        private static Dictionary<string, SpecialEventLayerContent> SpecialEventLayers { get; }
        private static KeyValuePair<string, SpecialEventLayerContent>[] SpecialEventLayersArray { get; }
        private static Timer UpdateSpecialEventTimer { get; }

        public static string CurrentInstanceName { get; private set; }
        public static string CurrentInstanceLocalizedName
        {
            get
            {
                var value = LocalizationUtils.GetLocalized(
                    GetEventLocalizationKey());

                if (string.IsNullOrEmpty(value))
                {
                    value = LocalizationUtils.GetLocalized(
                        "NothingTitle");
                }

                return value;
            }
        }
        public static SpecialEventLayerContent CurrentInstance { get; private set; }



        static SpecialEventManager()
        {
            SpecialEventLayers = new Dictionary<string, SpecialEventLayerContent>();
            SpecialEventLayersArray = Array.Empty<KeyValuePair<string, SpecialEventLayerContent>>();
            UpdateSpecialEventTimer = new Timer();

            CurrentInstanceName = null;
            CurrentInstance = null;

            foreach (var specialEventLayer in GetSpecialEventLayers())
            {
                var name = specialEventLayer.GetType().Name;
                name = name.Remove(name.Length - 5, 5);

                SpecialEventLayers.Add(name, specialEventLayer);
            }

            SpecialEventLayersArray = SpecialEventLayers.ToArray();

            UpdateSpecialEventTimer.Interval = TimeSpan.FromHours(1).TotalMilliseconds;
            UpdateSpecialEventTimer.Elapsed += UpdateSpecialEventTimer_Tick;

            UpdateSpecialEventTimer.Start();
        }



        private static string GetEventLocalizationKey()
        {
            return $"{CurrentInstanceName}Name";
        }

        private static SpecialEventLayerContent[] GetSpecialEventLayers()
        {
            try
            {
                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.IsClass
                                   && typeof(SpecialEventLayerContent).IsAssignableFrom(type)
                                   && type != typeof(SpecialEventLayerContent))
                    .ToArray();
                var specialEventLayers = new List<SpecialEventLayerContent>(types.Length);

                foreach (var type in types)
                {
                    var protocol =
                        Activator.CreateInstance(type, true) as SpecialEventLayerContent;

                    if (protocol == null)
                        continue;

                    specialEventLayers.Add(protocol);
                }

                return specialEventLayers.ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex, "Special event layers get error");

                return Array.Empty<SpecialEventLayerContent>();
            }
        }

#pragma warning disable SS002 // DateTime.Now was referenced
        private static bool LoadEvent(string specialEventName,
            SpecialEventLayerContent specialEventLayer)
        {
            var currentTime = DateTime.Now;

            if (!specialEventLayer.EventTimeSatisfied(currentTime)
                || !specialEventLayer.LoadEvent())
            {
                return false;
            }

            CurrentInstanceName = specialEventName;
            CurrentInstance = specialEventLayer;

            MainWindow.Instance.RootLayout.Children.Add(CurrentInstance);

            MainWindow.Instance.UpdateSpecialEventName();

            MainWindow.Instance.tglSpecialEvent.IsEnabled = true;

            MainWindow.Instance.SpecialEventEnabled = CurrentInstance.EventEnabled;
            MainWindow.Instance.BgmVolume = SettingsManager.AppSettings.BgmVolume;

            MainWindow.Instance.SpecialEventPanel.Visibility = Visibility.Visible;

            EventUpdated?.Invoke(null, EventArgs.Empty);

            return true;
        }
#pragma warning restore SS002 // DateTime.Now was referenced

        private static void UnloadEvent()
        {
            MainWindow.Instance.SpecialEventPanel.Visibility = Visibility.Collapsed;

            MainWindow.Instance.tglSpecialEvent.IsEnabled = false;

            if (CurrentInstance == null)
            {
                EventUpdated?.Invoke(null, EventArgs.Empty);

                return;
            }

            CurrentInstance.UnloadEvent();

            MainWindow.Instance.RootLayout.Children.Remove(CurrentInstance);

            CurrentInstanceName = null;
            CurrentInstance = null;

            MainWindow.Instance.UpdateSpecialEventName();

            EventUpdated?.Invoke(null, EventArgs.Empty);
        }



#pragma warning disable SS002 // DateTime.Now was referenced
        public static void UpdateEvent()
        {
            var currentTime = DateTime.Now;

            if (CurrentInstance != null)
            {
                var eventTimeSatisfied = CurrentInstance.EventTimeSatisfied(
                    currentTime);

                if (eventTimeSatisfied)
                    return;

                UnloadEvent();
            }

            foreach (var specialEventLayerPair in SpecialEventLayersArray)
            {
                var eventLoaded = LoadEvent(
                    specialEventLayerPair.Key,
                    specialEventLayerPair.Value);

                if (!eventLoaded)
                    continue;

                break;
            }
        }
#pragma warning restore SS002 // DateTime.Now was referenced

        public static void Activate(
            bool state)
        {
            CurrentInstance?.Activate(state);
        }

        public static void SetVolume(
            double value)
        {
            CurrentInstance?.SetVolume(value);
        }



        private static void UpdateSpecialEventTimer_Tick(object sender, EventArgs e)
        {
            MainWindow.Instance.Dispatcher.Invoke(() =>
            {
                UpdateEvent();
            });
        }
    }
}

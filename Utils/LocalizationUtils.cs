using System;
using System.Windows;
using Memenim.Localization;

namespace Memenim.Utils
{
    public static class LocalizationUtils
    {
        public static string GetLocalized(string key)
        {
            return GetLocalized<MainWindow, string>(MainWindow.Instance, key);
        }
        public static TOut GetLocalized<TOut>(string key)
        {
            return GetLocalized<MainWindow, TOut>(MainWindow.Instance, key);
        }
        public static string GetLocalized<TObject>(TObject element, string key)
            where TObject : FrameworkElement, ILocalizable
        {
            return GetLocalized<TObject, string>(element, key);
        }
        public static TOut GetLocalized<TObject, TOut>(TObject element, string key)
            where TObject : FrameworkElement, ILocalizable
        {
            return (TOut)element.FindResource(key);
        }

        public static string TryGetLocalized(string key)
        {
            return TryGetLocalized<MainWindow, string>(MainWindow.Instance, key);
        }
        public static TOut TryGetLocalized<TOut>(string key)
        {
            return TryGetLocalized<MainWindow, TOut>(MainWindow.Instance, key);
        }
        public static string TryGetLocalized<TObject>(TObject element, string key)
            where TObject : FrameworkElement, ILocalizable
        {
            return TryGetLocalized<TObject, string>(element, key);
        }
        public static TOut TryGetLocalized<TObject, TOut>(TObject element, string key)
            where TObject : FrameworkElement, ILocalizable
        {
            if (key == null)
                return default;

            return (TOut)element.TryFindResource(key);
        }
    }
}

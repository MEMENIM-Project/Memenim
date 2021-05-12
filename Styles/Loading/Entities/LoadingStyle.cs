using System;
using System.Windows;
using System.Windows.Media;

namespace Memenim.Styles.Loading.Entities
{
    public class LoadingStyle
    {
        public string StyleName { get; private set; }
        public ResourceDictionary Dictionary { get; private set; }


        public ResourceDictionary MahAppsDictionary { get; private set; }

        public SolidColorBrush TitleBackgroundBrush { get; private set; }
        public SolidColorBrush NonActiveTitleBackgroundBrush { get; private set; }
        public SolidColorBrush BackgroundBrush { get; private set; }

        public string ForegroundImagePath { get; private set; }
        public Uri ForegroundImageUri { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public Uri BackgroundImageUri { get; private set; }

        public HorizontalAlignment LoadingIndicatorHorizontalAlignment { get; private set; }
        public VerticalAlignment LoadingIndicatorVerticalAlignment { get; private set; }

        public LoadingStyle(string name,
            ResourceDictionary dictionary)
        {
            Load(name, dictionary);
        }

        public void Load(string name,
            ResourceDictionary dictionary)
        {
            T GetValue<T>(string key)
            {
                return (T)dictionary[key];
            }

            StyleName = name;
            Dictionary = dictionary;

            MahAppsDictionary = StylesManager.GetStyle(
                "MahApps", GetValue<string>("LoadingMahAppsThemeName"));

            TitleBackgroundBrush = GetValue<SolidColorBrush>("Window.Main.TitleBackground");
            NonActiveTitleBackgroundBrush = GetValue<SolidColorBrush>("Window.Main.NonActiveTitleBackground");
            BackgroundBrush = GetValue<SolidColorBrush>("Window.Main.Background");

            ForegroundImagePath = GetValue<string>("LoadingForegroundImagePath");

            if (!string.IsNullOrWhiteSpace(ForegroundImagePath)
                && !ForegroundImagePath.Equals("None", StringComparison.InvariantCultureIgnoreCase))
            {
                ForegroundImagePath = ForegroundImagePath.Replace('\\', '/');
                ForegroundImageUri = new Uri($"pack://application:,,,/{ForegroundImagePath}");
            }
            else
            {
                ForegroundImagePath = null;
                ForegroundImageUri = null;
            }

            BackgroundImagePath = GetValue<string>("LoadingBackgroundImagePath");

            if (!string.IsNullOrWhiteSpace(BackgroundImagePath)
                && !BackgroundImagePath.Equals("None", StringComparison.InvariantCultureIgnoreCase))
            {
                BackgroundImagePath = BackgroundImagePath.Replace('\\', '/');
                BackgroundImageUri = new Uri($"pack://application:,,,/{BackgroundImagePath}");
            }
            else
            {
                BackgroundImagePath = null;
                BackgroundImageUri = null;
            }

            LoadingIndicatorHorizontalAlignment = Enum.Parse<HorizontalAlignment>(
                GetValue<string>("LoadingIndicatorHorizontalAlignment"), true);
            LoadingIndicatorVerticalAlignment = Enum.Parse<VerticalAlignment>(
                GetValue<string>("LoadingIndicatorVerticalAlignment"), true);
        }
    }
}

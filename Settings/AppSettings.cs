using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using RIS.Settings;
using RIS.Settings.Ini;
using Environment = RIS.Environment;

namespace Memenim.Settings
{
    public class AppSettings : IniSettings
    {
        private const string SettingsFileName = "AppSettings.config";

        [ExcludedSetting]
        public object SyncRoot { get; }

        [SettingCategory("Localization")]
        public string Language { get; set; }
        [SettingCategory("Window")]
        public double WindowPositionX { get; set; }
        [SettingCategory("Window")]
        public double WindowPositionY { get; set; }
        [SettingCategory("Window")]
        public int WindowState { get; set; }
        [SettingCategory("Window")]
        public double WindowWidth { get; set; }
        [SettingCategory("Window")]
        public double WindowHeight { get; set; }
        [SettingCategory("Misc")]
        public bool SpecialEventEnabled { get; set; }
        [SettingCategory("Misc")]
        public double BgmVolume { get; set; }
        [SettingCategory("Log")]
        public int LogRetentionDaysPeriod { get; set; }
        [SettingCategory("Version")]
        public string AppVersion { get; set; }

        public AppSettings()
            : base(Path.Combine(Environment.ExecProcessDirectoryName, SettingsFileName))
        {
            SyncRoot = new object();

            Language = "en-US";
            WindowPositionX = SystemParameters.PrimaryScreenWidth / 2.0;
            WindowPositionY = SystemParameters.PrimaryScreenHeight / 2.0;
            WindowState = (int)System.Windows.WindowState.Normal;
            WindowWidth = MainWindow.Instance.Width;
            WindowHeight = MainWindow.Instance.Height;
            SpecialEventEnabled = true;
            BgmVolume = 0.5;
            LogRetentionDaysPeriod = 7;
            AppVersion = "0.0.0";

            Load();
        }

        public new void Load(SettingsLoadOptions options = SettingsLoadOptions.None)
        {
            lock (SyncRoot)
            {
                base.Load(options);

                string currentAppVersion = FileVersionInfo
                    .GetVersionInfo(Environment.ExecAppAssemblyFilePath)
                    .ProductVersion;

                if (AppVersion != currentAppVersion)
                {
                    base.Load(SettingsLoadOptions.RemoveUnused | SettingsLoadOptions.DeduplicatePreserveValues);
                    AppVersion = currentAppVersion;
                    Save();
                }
            }
        }

        public new void Save()
        {
            Task.Factory.StartNew(() =>
            {
                lock (SyncRoot)
                {
                    base.Save();
                }
            });
        }
    }
}

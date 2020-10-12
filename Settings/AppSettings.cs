using System.Threading.Tasks;
using RIS.Settings;
using RIS.Settings.Ini;

namespace AnonymDesktopClient.Core.Settings
{
    public class AppSettings : IniSettings
    {
        private const string SettingsFileName = "AppSettings.config";

        public object SyncRoot;

        public string Language { get; set; }
        public double WindowPositionX { get; set; }
        public double WindowPositionY { get; set; }
        public int WindowState { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }

        public AppSettings()
            : base(SettingsFileName)
        {
            SyncRoot = new object();

            Language = "en-US";
            WindowPositionX = System.Windows.SystemParameters.PrimaryScreenWidth / 2.0;
            WindowPositionY = System.Windows.SystemParameters.PrimaryScreenHeight / 2.0;
            WindowState = (int)System.Windows.WindowState.Normal;
            WindowWidth = MainWindow.CurrentMainWindow.Width;
            WindowHeight = MainWindow.CurrentMainWindow.Height;

            Load();
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

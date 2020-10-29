using System.Windows;
using Memenim.Settings;

namespace Memenim
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Memenim.MainWindow.Instance.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingsManager.AppSettings.Save();

            base.OnExit(e);
        }
    }
}

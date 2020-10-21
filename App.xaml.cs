using System.Windows;
using Memenim.Settings;

namespace Memenim
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingManager.AppSettings.Save();

            base.OnExit(e);
        }
    }
}

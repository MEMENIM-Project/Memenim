using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Memenim
{
    static class DialogManager
    {
        public static MetroWindow WindowRef
        {
            set { m_MainWindow = value; }
        }

        private static MetroWindow m_MainWindow;

        public static async Task ShowDialog(string title, string message)
        {
            await m_MainWindow.ShowMessageAsync(title, message);
        }

        public static async Task<string> ShowInputDialog(string title, string message)
        {
            return await m_MainWindow.ShowInputAsync(title, message);
        }
    }
}

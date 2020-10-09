using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace AnonymDesktopClient
{
    static class DialogManager
    {
        public static MetroWindow WindowRef
        {
            set { m_MainWindow = value; }
        }

        private static MetroWindow m_MainWindow;

        public static async void ShowDialog(string title, string message)
        {
            await m_MainWindow.ShowMessageAsync(title, message);
        }

        public static async Task<string> ShowInputDialog(string title, string message)
        {
            return await m_MainWindow.ShowInputAsync(title, message);
        }
    }
}

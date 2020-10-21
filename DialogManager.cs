using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace Memenim
{
    public static class DialogManager
    {
        public static Task<MessageDialogResult> ShowDialog(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            return MainWindow.CurrentInstance.ShowMessageAsync(title, message, style, settings);
        }

        public static Task<string> ShowInputDialog(string title, string message,
            MetroDialogSettings settings = null)
        {
            return MainWindow.CurrentInstance.ShowInputAsync(title, message, settings);
        }
    }
}

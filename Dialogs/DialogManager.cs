using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Logs;

namespace Memenim.Dialogs
{
    public static class DialogManager
    {
        public static Task<MessageDialogResult> ShowDialog(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            return MainWindow.Instance.ShowMessageAsync(title, message, style, settings);
        }

        public static Task<string> ShowInputDialog(string title, string message,
            MetroDialogSettings settings = null)
        {
            return MainWindow.Instance.ShowInputAsync(title, message, settings);
        }

        public static async Task<string> ShowPasswordDialog(string title, string message,
            bool canGeneratePassword = false, string defaultValue = null,
            MetroDialogSettings settings = null)
        {
            PasswordDialog dialog = new PasswordDialog(title,
                message, canGeneratePassword, defaultValue);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }

        public static async Task<string> ShowSinglelineTextDialog(string title, string message,
            string inputValue = "", string defaultValue = null,
            MetroDialogSettings settings = null)
        {
            SinglelineTextDialog dialog = new SinglelineTextDialog(title,
                message, inputValue, defaultValue);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }

        public static async Task<string> ShowMultilineTextDialog(string title, string message,
            string inputValue = "", string defaultValue = null,
            MetroDialogSettings settings = null)
        {
            MultilineTextDialog dialog = new MultilineTextDialog(title,
                message, inputValue, defaultValue);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }

        public static async Task<string> ShowComboBoxDialog(string title, string message,
            ReadOnlyCollection<string> values = null, string selectedValue = null,
            string defaultValue = null, MetroDialogSettings settings = null)
        {
            ComboBoxDialog dialog = new ComboBoxDialog(title,
                message, values, selectedValue, defaultValue);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.SelectedValue;
        }

        public static async Task<double?> ShowNumericDialog(string title, string message,
            double? inputValue = 0.0, double? minimumInputValue = 0.0,
            double? maximumInputValue = 100.0, double? intervalInputValue = 1.0,
            string stringFormatInputValue = "F0", double? defaultValue = null,
            MetroDialogSettings settings = null)
        {
            NumericDialog dialog = new NumericDialog(title,
                message, inputValue, minimumInputValue, maximumInputValue,
                intervalInputValue, stringFormatInputValue, defaultValue);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }
    }
}

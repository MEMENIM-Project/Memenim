using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Utils;
using RIS;
using RIS.Logging;

namespace Memenim.Dialogs
{
    public static class DialogManager
    {
        public static Task<MessageDialogResult> ShowDialog(string title,
            string message, MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            return MainWindow.Instance.ShowMessageAsync(
                title, message, style, settings);
        }

        public static async Task<MessageDialogResult> ShowMessageDialog(string title,
            string message, bool isCancellable = false,
            MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            MessageDialog dialog = new MessageDialog(title,
                message, isCancellable);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.DialogResult;
        }

        public static Task<MessageDialogResult> ShowConfirmationDialog(
            string additionalMessage = null, MetroDialogSettings settings = null)
        {
            string title = LocalizationUtils.TryGetLocalized("ConfirmationTitle")
                           ?? "Confirmation";
            string message = LocalizationUtils.TryGetLocalized("ConfirmationMessage")
                             ?? "Are you sure?";

            if (!string.IsNullOrEmpty(additionalMessage))
                message += " " + additionalMessage;

            return ShowMessageDialog(title, message,
                true, settings);
        }

        public static Task<MessageDialogResult> ShowErrorDialog(string message,
            bool isCancellable = false, MetroDialogSettings settings = null)
        {
            string title = LocalizationUtils.TryGetLocalized("ErrorTitle")
                           ?? "Error";

            Events.OnError(new RErrorEventArgs(
                $"{title} - {message}"));

            return ShowMessageDialog(title, message,
                isCancellable, settings);
        }

        public static Task<MessageDialogResult> ShowSuccessDialog(string message,
            bool isCancellable = false, MetroDialogSettings settings = null)
        {
            string title = LocalizationUtils.TryGetLocalized("SuccessTitle")
                           ?? "Success";

            return ShowMessageDialog(title, message,
                isCancellable, settings);
        }

        public static async Task<string> ShowPasswordDialog(string title,
            string message, bool canGeneratePassword = false, string defaultValue = null,
            bool isCancellable = true, MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            PasswordDialog dialog = new PasswordDialog(title,
                message, canGeneratePassword, defaultValue,
                isCancellable);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }

        public static async Task<string> ShowSinglelineTextDialog(string title,
            string message, string inputValue = "", string defaultValue = null,
            bool isCancellable = true, MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            SinglelineTextDialog dialog = new SinglelineTextDialog(title,
                message, inputValue, defaultValue, isCancellable);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }

        public static async Task<string> ShowMultilineTextDialog(string title,
            string message, string inputValue = "", string defaultValue = null,
            bool isCancellable = true, MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            MultilineTextDialog dialog = new MultilineTextDialog(title,
                message, inputValue, defaultValue, isCancellable);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }

        public static async Task<string> ShowComboBoxDialog(string title, string message,
            ReadOnlyCollection<string> values = null, string selectedValue = null,
            string defaultValue = null, bool isCancellable = true,
            MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            ComboBoxDialog dialog = new ComboBoxDialog(title,
                message, values, selectedValue, defaultValue,
                isCancellable);

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
            bool isCancellable = true, MetroDialogSettings settings = null)
        {
            LogManager.Log.Info($"{title} - {message}");

            NumericDialog dialog = new NumericDialog(title,
                message, inputValue, minimumInputValue, maximumInputValue,
                intervalInputValue, stringFormatInputValue, defaultValue,
                isCancellable);

            await MainWindow.Instance
                .ShowMetroDialogAsync(dialog, settings)
                .ConfigureAwait(true);

            await dialog.WaitUntilUnloadedAsync()
                .ConfigureAwait(true);

            return dialog.InputValue;
        }
    }
}

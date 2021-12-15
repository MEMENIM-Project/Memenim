using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Generating;
using Memenim.Utils;

namespace Memenim.Dialogs
{
    public partial class PasswordDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(PasswordDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(PasswordDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.Register(nameof(InputValue), typeof(string), typeof(PasswordDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsCancellableProperty =
            DependencyProperty.Register(nameof(IsCancellable), typeof(bool), typeof(PasswordDialog),
                new PropertyMetadata(true));



        public string DialogTitle
        {
            get
            {
                return (string)GetValue(DialogTitleProperty);
            }
            set
            {
                SetValue(DialogTitleProperty, value);
            }
        }
        public string DialogMessage
        {
            get
            {
                return (string)GetValue(DialogMessageProperty);
            }
            set
            {
                SetValue(DialogMessageProperty, value);
            }
        }
        public string InputValue
        {
            get
            {
                return (string)GetValue(InputValueProperty);
            }
            set
            {
                SetValue(InputValueProperty, value);
            }
        }
        public bool IsCancellable
        {
            get
            {
                return (bool)GetValue(IsCancellableProperty);
            }
            set
            {
                SetValue(IsCancellableProperty, value);
            }
        }
        public string DefaultValue { get; }
        public bool CanGeneratePassword { get; }



        public PasswordDialog(
            string title = "Enter",
            string message = "Enter",
            bool canGeneratePassword = false,
            string defaultValue = null,
            bool isCancellable = true)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            DefaultValue = defaultValue;
            CanGeneratePassword = canGeneratePassword;
            IsCancellable = isCancellable;

            if (!LocalizationUtils.TryGetLocalized("OkTitle", out _))
                OkButton.Content = "Ok";
            if (!LocalizationUtils.TryGetLocalized("CancelTitle", out _))
                CancelButton.Content = "Cancel";
        }



        private void Dialog_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (OkButton.IsEnabled)
                    Ok_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                if (CancelButton.IsEnabled)
                    Cancel_Click(this, new RoutedEventArgs());
            }
        }



        private void Ok_Click(object sender,
            RoutedEventArgs e)
        {
            OkButton.Focus();

            MainWindow.Instance.HideMetroDialogAsync(
                this, DialogSettings);
        }

        private void Cancel_Click(object sender,
            RoutedEventArgs e)
        {
            CancelButton.Focus();

            InputValue = DefaultValue;

            MainWindow.Instance.HideMetroDialogAsync(
                this, DialogSettings);
        }

        private void PasswordTextBox_OnPasswordChanged(object sender,
            RoutedEventArgs e)
        {
            InputValue = PasswordTextBox.Password;
        }

        private async void GeneratePasswordButton_Click(object sender,
            RoutedEventArgs e)
        {
            OkButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;

            try
            {
                var password = GeneratingManager.RandomStringGenerator
                    .GenerateString(20);

                if (password == null)
                {
                    var message = LocalizationUtils
                        .GetLocalized("CopyingToClipboardErrorMessage");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                PasswordTextBox.Password = password;
                InputValue = password;

                Clipboard.SetText(
                    password);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                OkButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
            }
        }
    }
}

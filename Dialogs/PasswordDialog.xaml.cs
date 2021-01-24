using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Utils;
using RIS.Text.Generating;

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

        public PasswordDialog(string title = "Enter", string message = "Enter",
            bool canGeneratePassword = false, string defaultValue = null,
            bool isCancellable = true)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            DefaultValue = defaultValue;
            CanGeneratePassword = canGeneratePassword;
            IsCancellable = isCancellable;

            if (string.IsNullOrEmpty(LocalizationUtils.TryGetLocalized("OkTitle")))
                btnOk.Content = "Ok";
            if (string.IsNullOrEmpty(LocalizationUtils.TryGetLocalized("CancelTitle")))
                btnCancel.Content = "Cancel";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            btnOk.Focus();

            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            btnCancel.Focus();

            InputValue = DefaultValue;
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }

        private void txtPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            InputValue = txtPassword.Password;
        }

        private async void btnGeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            btnOk.IsEnabled = false;
            btnCancel.IsEnabled = false;
            txtPassword.IsEnabled = false;

            try
            {
                string password = StringGenerator.GenerateString(20);

                txtPassword.Password = password;
                InputValue = password;
                Clipboard.SetText(password);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnOk.IsEnabled = true;
                btnCancel.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }
        }

        private void Dialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (btnOk.IsEnabled)
                    Ok_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape || e.Key == Key.Up)
            {
                if (btnCancel.IsEnabled)
                    Cancel_Click(this, new RoutedEventArgs());
            }
        }
    }
}

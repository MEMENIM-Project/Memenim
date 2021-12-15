using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Utils;

namespace Memenim.Dialogs
{
    public partial class MessageDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(MessageDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(MessageDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.Register(nameof(DialogResult), typeof(MessageDialogResult), typeof(MessageDialog),
                new PropertyMetadata(MessageDialogResult.Affirmative));
        public static readonly DependencyProperty IsCancellableProperty =
            DependencyProperty.Register(nameof(IsCancellable), typeof(bool), typeof(MessageDialog),
                new PropertyMetadata(false));



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
        public MessageDialogResult DialogResult
        {
            get
            {
                return (MessageDialogResult)GetValue(DialogResultProperty);
            }
            set
            {
                SetValue(DialogResultProperty, value);
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



        public MessageDialog(
            string title = "Message",
            string message = "Message",
            bool isCancellable = false)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
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

            DialogResult = MessageDialogResult.Affirmative;

            MainWindow.Instance.HideMetroDialogAsync(
                this, DialogSettings);
        }

        private void Cancel_Click(object sender,
            RoutedEventArgs e)
        {
            CancelButton.Focus();

            DialogResult = MessageDialogResult.Negative;

            MainWindow.Instance.HideMetroDialogAsync(
                this, DialogSettings);
        }
    }
}

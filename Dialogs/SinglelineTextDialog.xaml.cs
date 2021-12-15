using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Utils;

namespace Memenim.Dialogs
{
    public partial class SinglelineTextDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(SinglelineTextDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(SinglelineTextDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.Register(nameof(InputValue), typeof(string), typeof(SinglelineTextDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsCancellableProperty =
            DependencyProperty.Register(nameof(IsCancellable), typeof(bool), typeof(SinglelineTextDialog),
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



        public SinglelineTextDialog(
            string title = "Enter",
            string message = "Enter",
            string inputValue = "",
            string defaultValue = null,
            bool isCancellable = true)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            InputValue = inputValue;
            DefaultValue = defaultValue;
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
    }
}

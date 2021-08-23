using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Utils;

namespace Memenim.Dialogs
{
    public partial class MultilineTextDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(MultilineTextDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(MultilineTextDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.Register(nameof(InputValue), typeof(string), typeof(MultilineTextDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsCancellableProperty =
            DependencyProperty.Register(nameof(IsCancellable), typeof(bool), typeof(MultilineTextDialog),
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

        public MultilineTextDialog(string title = "Enter", string message = "Enter",
            string inputValue = "", string defaultValue = null, bool isCancellable = true)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            InputValue = inputValue;
            DefaultValue = defaultValue;
            IsCancellable = isCancellable;

            if (!LocalizationUtils.TryGetLocalized("OkTitle", out _))
                btnOk.Content = "Ok";
            if (!LocalizationUtils.TryGetLocalized("CancelTitle", out _))
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

        private void Dialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Enter)
            {
                if (btnOk.IsEnabled)
                    Ok_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                if (btnCancel.IsEnabled)
                    Cancel_Click(this, new RoutedEventArgs());
            }
        }
    }
}

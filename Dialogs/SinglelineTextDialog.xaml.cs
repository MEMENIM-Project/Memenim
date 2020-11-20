using System;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

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
        public string DefaultValue { get; }

        public SinglelineTextDialog(string title = "Enter", string message = "Enter",
            string inputValue = "", string defaultValue = null)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            InputValue = inputValue;
            DefaultValue = defaultValue;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            InputValue = DefaultValue;
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }
    }
}

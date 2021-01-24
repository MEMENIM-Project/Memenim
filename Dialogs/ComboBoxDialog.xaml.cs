using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Memenim.Utils;

namespace Memenim.Dialogs
{
    public partial class ComboBoxDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(ComboBoxDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(ComboBoxDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(nameof(Values), typeof(ReadOnlyCollection<string>), typeof(ComboBoxDialog),
                new PropertyMetadata(new ReadOnlyCollection<string>(new List<string>())));
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(nameof(SelectedValue), typeof(string), typeof(ComboBoxDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsCancellableProperty =
            DependencyProperty.Register(nameof(IsCancellable), typeof(bool), typeof(ComboBoxDialog),
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
        public ReadOnlyCollection<string> Values
        {
            get
            {
                return (ReadOnlyCollection<string>)GetValue(ValuesProperty);
            }
            set
            {
                SetValue(ValuesProperty, value);
            }
        }
        public string SelectedValue
        {
            get
            {
                return (string)GetValue(SelectedValueProperty);
            }
            set
            {
                SetValue(SelectedValueProperty, value);
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

        public ComboBoxDialog(string title = "Enter", string message = "Enter",
            ReadOnlyCollection<string> values = null, string selectedValue = null,
            string defaultValue = null, bool isCancellable = true)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            Values = values;
            DefaultValue = defaultValue;
            IsCancellable = isCancellable;

            if (selectedValue != null)
                lstValues.SelectedItem = selectedValue;

            if (string.IsNullOrEmpty(LocalizationUtils.TryGetLocalized("OkTitle")))
                btnOk.Content = "Ok";
            if (string.IsNullOrEmpty(LocalizationUtils.TryGetLocalized("CancelTitle")))
                btnCancel.Content = "Cancel";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            btnOk.Focus();

            SelectedValue = (string)lstValues.SelectedItem;
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            btnCancel.Focus();

            SelectedValue = DefaultValue;
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
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

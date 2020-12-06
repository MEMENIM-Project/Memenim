using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;

namespace Memenim.Dialogs
{
    public partial class NumericDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(NumericDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(NumericDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.Register(nameof(InputValue), typeof(double?), typeof(NumericDialog),
                new PropertyMetadata(0.0));
        public static readonly DependencyProperty MinimumInputValueProperty =
            DependencyProperty.Register(nameof(MinimumInputValue), typeof(double?), typeof(NumericDialog),
                new PropertyMetadata(0.0));
        public static readonly DependencyProperty MaximumInputValueProperty =
            DependencyProperty.Register(nameof(MaximumInputValue), typeof(double?), typeof(NumericDialog),
                new PropertyMetadata(100.0));
        public static readonly DependencyProperty IntervalInputValueProperty =
            DependencyProperty.Register(nameof(IntervalInputValue), typeof(double?), typeof(NumericDialog),
                new PropertyMetadata(1.0));
        public static readonly DependencyProperty StringFormatInputValueProperty =
            DependencyProperty.Register(nameof(StringFormatInputValue), typeof(string), typeof(NumericDialog),
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
        public double? InputValue
        {
            get
            {
                return (double?)GetValue(InputValueProperty);
            }
            set
            {
                SetValue(InputValueProperty, value);
            }
        }
        public double? MinimumInputValue
        {
            get
            {
                return (double?)GetValue(MinimumInputValueProperty);
            }
            set
            {
                SetValue(MinimumInputValueProperty, value);
            }
        }
        public double? MaximumInputValue
        {
            get
            {
                return (double?)GetValue(MaximumInputValueProperty);
            }
            set
            {
                SetValue(MaximumInputValueProperty, value);
            }
        }
        public double? IntervalInputValue
        {
            get
            {
                return (double?)GetValue(IntervalInputValueProperty);
            }
            set
            {
                SetValue(IntervalInputValueProperty, value);
            }
        }
        public string StringFormatInputValue
        {
            get
            {
                return (string)GetValue(StringFormatInputValueProperty);
            }
            set
            {
                SetValue(StringFormatInputValueProperty, value);
            }
        }
        public double? DefaultValue { get; }

        public NumericDialog(string title = "Enter", string message = "Enter",
            double? inputValue = 0.0, double? minimumInputValue = 0.0,
            double? maximumInputValue = 100.0,  double? intervalInputValue = 1.0,
            string stringFormatInputValue = "F0", double? defaultValue = null)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            InputValue = inputValue;
            MinimumInputValue = minimumInputValue;
            MaximumInputValue = maximumInputValue;
            IntervalInputValue = intervalInputValue;
            StringFormatInputValue = stringFormatInputValue;
            DefaultValue = defaultValue;
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

using System;
using System.Windows;
using Memenim.Dialogs;
using Memenim.Utils;

namespace Memenim.Widgets
{
    public partial class UserProfileStat : WidgetContent
    {
        public static readonly RoutedEvent EditClickEvent =
            EventManager.RegisterRoutedEvent(nameof(EditClick), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(UserProfileStat));
        


        public static readonly DependencyProperty StatTitleProperty =
            DependencyProperty.Register(nameof(StatTitle), typeof(string), typeof(UserProfileStat),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty StatValueProperty =
            DependencyProperty.Register(nameof(StatValue), typeof(string), typeof(UserProfileStat),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty EditAllowedProperty =
            DependencyProperty.Register(nameof(EditAllowed), typeof(bool), typeof(UserProfileStat),
                new PropertyMetadata(false));



        public event EventHandler<RoutedEventArgs> EditClick
        {
            add
            {
                AddHandler(EditClickEvent, value);
            }
            remove
            {
                RemoveHandler(EditClickEvent, value);
            }
        }



        public string StatTitle
        {
            get
            {
                return (string)GetValue(StatTitleProperty);
            }
            set
            {
                SetValue(StatTitleProperty, value);
            }
        }
        public string StatValue
        {
            get
            {
                return (string)GetValue(StatValueProperty);
            }
            set
            {
                SetValue(StatValueProperty, value);
            }
        }
        public bool EditAllowed
        {
            get
            {
                return (bool)GetValue(EditAllowedProperty);
            }
            set
            {
                SetValue(EditAllowedProperty, value);
            }
        }



        public UserProfileStat()
        {
            InitializeComponent();
            DataContext = this;
        }



        private async void CopyProfileStatText_Click(object sender,
            RoutedEventArgs e)
        {
            var text = StatValue;

            if (text == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(text);
        }



        private void Edit_Click(object sender,
            RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(EditClickEvent));
        }
    }
}

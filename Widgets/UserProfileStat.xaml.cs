using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Widgets
{
    public partial class UserProfileStat : UserControl
    {
        public static readonly RoutedEvent OnEditClicked =
            EventManager.RegisterRoutedEvent(nameof(EditClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(UserProfileStat));
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
                AddHandler(OnEditClicked, value);
            }
            remove
            {
                RemoveHandler(OnEditClicked, value);
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

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnEditClicked));
        }
    }
}

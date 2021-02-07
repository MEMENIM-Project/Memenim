using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Schema;

namespace Memenim.Widgets
{
    public partial class UserStatus : UserControl
    {
        public static readonly DependencyProperty StatusValueProperty =
            DependencyProperty.Register(nameof(StatusValue), typeof(UserStatusType), typeof(UserStatus),
                new PropertyMetadata(UserStatusType.Active));

        public UserStatusType StatusValue
        {
            get
            {
                return (UserStatusType)GetValue(StatusValueProperty);
            }
            set
            {
                SetValue(StatusValueProperty, value);
            }
        }

        public UserStatus()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}

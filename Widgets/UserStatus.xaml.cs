using System;
using System.Windows;
using Memenim.Core.Schema;

namespace Memenim.Widgets
{
    public partial class UserStatus : WidgetContent
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

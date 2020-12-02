using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for UserStatus.xaml
    /// </summary>
    /// 

    public enum EUserStatus
    {
        Active = 0,
        Banned = 2,
        Moderator = 9,
        Admin = 10
    }

    public partial class UserStatus : UserControl
    {
        public static readonly DependencyProperty StatusValueProperty =
            DependencyProperty.Register(nameof(StatusValue), typeof(EUserStatus), typeof(UserStatus),
                new PropertyMetadata(EUserStatus.Active));

        public EUserStatus StatusValue
        {
            get 
            {
                return (EUserStatus)GetValue(StatusValueProperty);
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

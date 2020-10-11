using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for UserProfileStat.xaml
    /// </summary>
    public partial class UserProfileStat : UserControl
    {
        public string StatTitle { get; set; }

        public string StatValue 
        { 
            get { return (string)GetValue(StatValueProperty); }
            set { SetValue(StatValueProperty, value); }
        }

        public static readonly DependencyProperty StatValueProperty = DependencyProperty.Register("StatValue",
                                                                      typeof(string), 
                                                                      typeof(UserProfileStat),
                                                                      new PropertyMetadata("#NOT_FOUND"));

        public UserProfileStat()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}

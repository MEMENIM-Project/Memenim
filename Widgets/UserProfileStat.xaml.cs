using System.Windows;
using System.Windows.Controls;

namespace AnonymDesktopClient.Core.Widgets
{
    /// <summary>
    /// Interaction logic for UserProfileStat.xaml
    /// </summary>
    public partial class UserProfileStat : UserControl
    {
        public string StatTitle { get; set; }

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

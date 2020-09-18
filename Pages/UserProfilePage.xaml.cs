using Memenim.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for AccountViewPage.xaml
    /// </summary>
    public partial class UserProfilePage : UserControl
    {
        private ProfileData m_UserInfo;

        public UserProfilePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            m_UserInfo = GeneralBlackboard.TryGetValue<ProfileData>(BlackBoardValues.EProfileData);
            DataContext = m_UserInfo;
        }

    }
}

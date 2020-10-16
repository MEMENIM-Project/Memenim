using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for AccountViewPage.xaml
    /// </summary>
    public partial class UserProfilePage : UserControl
    {
        public int UserID { get; set; }

        private ProfileData m_UserInfo { get; set; }

        public UserProfilePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var res = await UserApi.GetProfileById(UserID);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            m_UserInfo = res.data[0];
            DataContext = m_UserInfo;
        }
    }
}

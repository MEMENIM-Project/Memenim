using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for AccountViewPage.xaml
    /// </summary>
    public partial class UserProfilePage : PageContent
    {
        public int UserID { get; set; }

        private ProfileData m_UserInfo { get; set; }

        public UserProfilePage() : base()
        {
            InitializeComponent();
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            UserID = (int)DataContext.GetType().GetProperty("UserID").GetValue(DataContext, null);
            var res = await UserApi.GetProfileById(UserID);
            if (res.error)
            {
                await DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            m_UserInfo = res.data[0];
            DataContext = m_UserInfo;
        }
    }
}

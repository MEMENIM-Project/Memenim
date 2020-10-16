using System.Windows;
using System.Windows.Controls;
using Memenim.Pages;
using Memenim.Utils;
using Memenim.Core.Api;
using Memenim.Core.Data;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for ProfileBanner.xaml
    /// </summary>
    public partial class ProfileBanner : UserControl
    {
        public ProfileData UserProfile { get; private set; }


        public ProfileBanner()
        {
            InitializeComponent();
        }

        private async void UserBanner_Loaded(object sender, RoutedEventArgs e)
        {
            var profile = await UserApi.GetProfileById(GeneralBlackboard.TryGetValue<int>(BlackBoardValues.EProfileData));
            UserProfile = profile.data[0];

            DataContext = UserProfile;
        }

        private void SelectAvatarFromTennor_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, new SettingsPage());
            PageNavigationManager.SwitchToSubpage(new TennorSearchPage() { OnPicSelect = ProfileUtils.ChangeAvatar });
        }

        private void SelectBannerFromTennor_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, new SettingsPage());
            PageNavigationManager.SwitchToSubpage(new TennorSearchPage() { OnPicSelect = ProfileUtils.ChangeBanner });
        }

        private async void SelectAvatarFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL");
            await ProfileUtils.ChangeAvatar(url);
        }
    }
}

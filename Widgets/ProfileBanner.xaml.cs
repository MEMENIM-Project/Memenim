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
using Memenim.Core.Data;
using AnonymDesktopClient.Pages;
using AnonymDesktopClient.Core.Pages;
using AnonymDesktopClient.Core.Utils;
using Memenim.Core;

namespace AnonymDesktopClient.Core.Widgets
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
            var profile = await UsersAPI.GetUserProfileByID(GeneralBlackboard.TryGetValue<int>(BlackBoardValues.EProfileData));
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
    }
}

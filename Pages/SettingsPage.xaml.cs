using AnonymDesktopClient.Core;

using Memenim.Core;
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

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public string Username { get; set; } = "Undefined";
        public string Login { get; set; } = "Undefined";
        public string AvatarURL { get; set; }

        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var profile = await UsersAPI.GetUserProfileByID(AppPersistent.LocalUserId);
            Login = profile.data[0].login;
            Username = profile.data[0].name;
            AvatarURL = profile.data[0].photo;
            DataContext = this;
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new LoginPage());
        }

        private async void btnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var profile = await UsersAPI.GetUserProfileByID(AppPersistent.LocalUserId);
                if(!profile.error)
                {
                    profile.data[0].photo = txtPicURL.Text;
                    var res = await UsersAPI.EditProfile(profile.data[0], AppPersistent.UserToken);
                    if (!res.error)
                    {
                        DialogManager.ShowDialog("S U C C", "You changed your avatar.");
                    }
                    else
                    {
                        DialogManager.ShowDialog("F U C K", res.message);
                    }
                }
                else
                {
                    DialogManager.ShowDialog("F U C K", profile.message);
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("F U C K", ex.Message);
            }
        }

        private async void btnChangeBanner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var profile = await UsersAPI.GetUserProfileByID(AppPersistent.LocalUserId);
                if (!profile.error)
                {
                    profile.data[0].photo = txtPicURL.Text;
                    var res = await UsersAPI.EditProfile(profile.data[0], AppPersistent.UserToken);
                    if (!res.error)
                    {
                        DialogManager.ShowDialog("S U C C", "You changed your avatar.");
                    }
                    else
                    {
                        DialogManager.ShowDialog("F U C K", res.message);
                    }
                }
                else
                {
                    DialogManager.ShowDialog("F U C K", profile.message);
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("F U C K", ex.Message);
            }
        }
    }
}

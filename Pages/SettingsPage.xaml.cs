using AnonymDesktopClient.DataStructs;
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
            ProfileData profile = await ApiHelper.GetLocalUserInfo();
            Login = profile.login;
            Username = profile.name;
            AvatarURL = profile.photo;
            DataContext = this;

        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new LoginPage());
        }
    }
}

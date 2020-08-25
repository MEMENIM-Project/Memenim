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

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            var res = await ApiHelper.Auth(txtLogin.Text, txtPass.Text);

            if(res != null)
            {
                lblStatus.Content = res;
                btnLogin.IsEnabled = true;
            }
            else
            {
                NavigationService.Navigate(new Uri("PostsPage.xaml", UriKind.Relative));
            }
        }
    }
}

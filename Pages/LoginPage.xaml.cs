using AnonymDesktopClient.DataStructs;
using AnonymDesktopClient.Pages;
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
    public partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            try
            {
                var res = await ApiHelper.Auth(txtLogin.Text, txtPass.Password);

                if (res != null)
                {
                    lblStatus.Content = res;
                    btnLogin.IsEnabled = true;
                }
                else
                {
                    PageNavigationManager.SwitchToPage(new ApplicationPage());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "An exception happened");
            }

        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new RegisterUser());
        }
    }
}

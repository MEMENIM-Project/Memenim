
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
using Memenim.Core;
using AnonymDesktopClient.Core;

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
                var result = await UsersAPI.Login(txtLogin.Text, txtPass.Password);

                if(result.error)
                {
                    lblStatus.Content = result.message;
                }
                else
                {
                    AppPersistent.UserToken = result.data.token;
                    AppPersistent.LocalUserId = result.data.id;
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

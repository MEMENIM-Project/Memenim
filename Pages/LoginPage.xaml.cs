
using AnonymDesktopClient.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

            btnLogin.IsEnabled = false;
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;

            try
            {
                var result = await UsersAPI.Login(txtLogin.Text, txtPassword.Password).ConfigureAwait(true);

                if(result.error)
                {
                    lblStatus.Content = result.message;
                    btnLogin.IsEnabled = true;
                }
                else
                {
                    if (chkRememberMe.IsChecked.GetValueOrDefault())
                    {
                        AppPersistent.AddToStore("UserToken", AppPersistent.WinProtect(result.data.token, "UserToken"));
                        AppPersistent.AddToStore("UserId", AppPersistent.WinProtect(result.data.id.ToString(), "UserId"));
                    }
                    else
                    {
                        AppPersistent.RemoveFromStore("UserToken");
                        AppPersistent.RemoveFromStore("UserId");
                    }

                    AppPersistent.UserToken = result.data.token;
                    AppPersistent.LocalUserId = result.data.id;
                    PageNavigationManager.SwitchToPage(new ApplicationPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "An exception happened");
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new RegisterUser());
        }

        private void txtLogin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (btnLogin.IsEnabled)
                    btnLogin_Click(null, new RoutedEventArgs());
            }
            else if (e.Key == Key.Up)
            {
                txtLogin.Focus();
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = !NeedBlockLogin();
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnLogin.IsEnabled = !NeedBlockLogin();
        }

        private bool NeedBlockLogin()
        {
            return txtPassword.Password.Length == 0 || txtLogin.Text.Length == 0;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;

namespace Memenim.Pages
{
    public partial class LoginPage : PageContent
    {
        public LoginViewModel ViewModel
        {
            get
            {
                return DataContext as LoginViewModel;
            }
        }

        public LoginPage()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();

            btnLogin.IsEnabled = false;
        }

        private bool NeedBlockLogin()
        {
            return txtPassword.Password.Length == 0 || txtLogin.Text.Length == 0;
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            btnGoToRegister.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;

            try
            {
                var result = await UserApi.Login(txtLogin.Text, txtPassword.Password)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    lblErrorMessage.Content = result.message;

                    txtPassword.Clear();
                }
                else
                {
                    if (chkRememberMe.IsChecked == true)
                    {
                        SettingsManager.PersistentSettings.SetUser(txtLogin.Text,
                            PersistentUtils.WinProtect(result.data.token, $"UserToken-{txtLogin.Text}"),
                            PersistentUtils.WinProtect(result.data.id.ToString(), $"UserId-{txtLogin.Text}"));
                        SettingsManager.PersistentSettings.SetCurrentUserLogin(txtLogin.Text);
                    }
                    else
                    {
                        SettingsManager.PersistentSettings.RemoveUser(txtLogin.Text);
                    }

                    SettingsManager.PersistentSettings.CurrentUserLogin = txtLogin.Text;
                    SettingsManager.PersistentSettings.CurrentUserToken = result.data.token;
                    SettingsManager.PersistentSettings.CurrentUserId = result.data.id;

                    NavigationController.Instance.RequestPage<FeedPage>();

                    txtLogin.Clear();
                    txtPassword.Clear();
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("An exception happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnGoToRegister.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }
        }

        private void btnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<RegisterPage>();

            txtLogin.Clear();
            txtPassword.Clear();
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
                    btnLogin_Click(this, new RoutedEventArgs());
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
    }
}

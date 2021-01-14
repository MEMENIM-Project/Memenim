using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Settings.Entities;

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

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            txtLogin.Clear();
            txtPassword.Clear();
            chkRememberMe.IsChecked = false;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            base.OnExit(sender, e);
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            btnGoToRegister.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;
            chkRememberMe.IsEnabled = false;

            try
            {
                var result = await UserApi.Login(txtLogin.Text, txtPassword.Password)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowDialog("Login error", result.message)
                        .ConfigureAwait(true);
                }
                else
                {
                    if (chkRememberMe.IsChecked == true)
                    {
                        if (!SettingsManager.PersistentSettings.SetUser(
                            txtLogin.Text,
                            result.data.token,
                            result.data.id))
                        {
                            return;
                        }
                        if (!SettingsManager.PersistentSettings.SetCurrentUser(
                            txtLogin.Text))
                        {
                            return;
                        }
                    }
                    else
                    {
                        SettingsManager.PersistentSettings.RemoveUser(
                            txtLogin.Text);

                        if (!SettingsManager.PersistentSettings.SetCurrentUser(
                            new User(
                                txtLogin.Text,
                                result.data.token,
                                result.data.id)))
                        {
                            return;
                        }
                    }

                    NavigationController.Instance.RequestPage<FeedPage>();

                    txtLogin.Clear();
                    chkRememberMe.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("An exception happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                txtPassword.Clear();

                btnLogin.IsEnabled = !NeedBlockLogin();
                btnGoToRegister.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
                chkRememberMe.IsEnabled = true;
            }
        }

        private void btnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<RegisterPage>();
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for RegisterUser.xaml
    /// </summary>
    public partial class RegisterUser : UserControl
    {
        public RegisterUser()
        {
            InitializeComponent();

            btnRegister.IsEnabled = false;
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;

            try
            {
                var result = await UserApi.Register(txtLogin.Text, txtPassword.Password).ConfigureAwait(true);

                if (result.error)
                {
                    lblErrorMessage.Text = result.message;
                    btnRegister.IsEnabled = true;
                }
                else
                {
                    AppPersistent.AddToStore("UserToken", AppPersistent.WinProtect(result.data.token, "UserToken"));
                    AppPersistent.AddToStore("UserId", AppPersistent.WinProtect(result.data.id.ToString(), "UserId"));

                    AppPersistent.UserToken = result.data.token;
                    AppPersistent.LocalUserId = result.data.id;
                    PageNavigationManager.SwitchToPage(new ApplicationPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnAutoReg_Click(object sender, RoutedEventArgs e)
        {
            //btnAutoReg.IsEnabled = false;

            const string name = "bot";
            ulong counter = 0;

            var result = await UserApi.Register(name + counter.ToString("D10"), "botpass").ConfigureAwait(true);

            while (result.error)
            {
                ++counter;
                result = await UserApi.Register(name + counter.ToString("D10"), "botpass").ConfigureAwait(true);
            }

            AppPersistent.AddToStore("UserToken", AppPersistent.WinProtect(result.data.token, "UserToken"));
            AppPersistent.AddToStore("UserId", AppPersistent.WinProtect(result.data.id.ToString(), "UserId"));

            AppPersistent.UserToken = result.data.token;
            AppPersistent.LocalUserId = result.data.id;

            DialogManager.ShowDialog("S U C C", "Registered user with username " + name + counter.ToString("D10"));
            PageNavigationManager.SwitchToPage(new ApplicationPage());
        }

        private void btnGoToLogin_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new LoginPage());
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
                if (btnRegister.IsEnabled)
                    btnRegister_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Up)
            {
                txtLogin.Focus();
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = !NeedBlockRegister();
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnRegister.IsEnabled = !NeedBlockRegister();
        }

        private bool NeedBlockRegister()
        {
            return txtPassword.Password.Length == 0 || txtLogin.Text.Length == 0;
        }
    }
}

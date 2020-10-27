using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Managers;
using Memenim.Settings;
using Memenim.Utils;

namespace Memenim.Pages
{
    public partial class RegisterUser : PageContent
    {
        public RegisterUser()
            : base()
        {
            InitializeComponent();
            DataContext = this;

            btnRegister.IsEnabled = false;
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;

            try
            {
                var result = await UserApi.Register(txtLogin.Text, txtPassword.Password)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    lblErrorMessage.Text = result.message;

                    txtPassword.Clear();
                }
                else
                {
                    SettingManager.PersistentSettings.SetUser(txtLogin.Text,
                        PersistentUtils.WinProtect(result.data.token, $"UserToken-{txtLogin.Text}"),
                        PersistentUtils.WinProtect(result.data.id.ToString(), $"UserId-{txtLogin.Text}"));
                    SettingManager.PersistentSettings.SetCurrentUserLogin(txtLogin.Text);

                    SettingManager.PersistentSettings.CurrentUserLogin = txtLogin.Text;
                    SettingManager.PersistentSettings.CurrentUserToken = result.data.token;
                    SettingManager.PersistentSettings.CurrentUserId = result.data.id;

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
                btnRegister.IsEnabled = true;
            }
        }

        private async void btnAutoReg_Click(object sender, RoutedEventArgs e)
        {
            //btnAutoReg.IsEnabled = false;

            const string name = "bot";
            ulong counter = 0;

            var result = await UserApi.Register(name + counter.ToString("D10"), "botpass")
                .ConfigureAwait(true);

            while (result.error)
            {
                ++counter;
                result = await UserApi.Register(name + counter.ToString("D10"), "botpass")
                    .ConfigureAwait(true);
            }

            await DialogManager.ShowDialog("S U C C", "Registered user with username " + name + counter.ToString("D10"))
                .ConfigureAwait(true);

            SettingManager.PersistentSettings.SetUser(txtLogin.Text,
                PersistentUtils.WinProtect(result.data.token, $"UserToken-{txtLogin.Text}"),
                PersistentUtils.WinProtect(result.data.id.ToString(), $"UserId-{txtLogin.Text}"));
            SettingManager.PersistentSettings.SetCurrentUserLogin(txtLogin.Text);

            SettingManager.PersistentSettings.CurrentUserLogin = txtLogin.Text;
            SettingManager.PersistentSettings.CurrentUserToken = result.data.token;
            SettingManager.PersistentSettings.CurrentUserId = result.data.id;

            NavigationController.Instance.RequestPage<FeedPage>();

            txtLogin.Clear();
            txtPassword.Clear();

            //btnAutoReg.IsEnabled = true;
        }

        private void btnGoToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<LoginPage>();
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

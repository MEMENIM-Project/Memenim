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
using RIS.Text.Generating;

namespace Memenim.Pages
{
    public partial class RegisterPage : PageContent
    {
        private bool _changeNicknameExplicit;

        public RegisterViewModel ViewModel
        {
            get
            {
                return DataContext as RegisterViewModel;
            }
        }

        public RegisterPage()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();

            btnRegister.IsEnabled = false;
        }

        private bool NeedBlockRegister()
        {
            return txtPassword.Password.Length == 0 || txtLogin.Text.Length == 0;
        }

        private async void btnGeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;
            //btnAutoRegister.IsEnabled = false;
            btnGoToLogin.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;

            try
            {
                string password = StringGenerator.GenerateString(20);

                txtPassword.Password = password;
                Clipboard.SetText(password);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("An exception happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnRegister.IsEnabled = !NeedBlockRegister();
                //btnAutoRegister.IsEnabled = true;
                btnGoToLogin.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;
            //btnAutoRegister.IsEnabled = false;
            btnGoToLogin.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtNickname.IsEnabled = false;

            try
            {
                string nickname = txtNickname.Text;

                if (string.IsNullOrWhiteSpace(nickname))
                    nickname = null;

                var result = await UserApi.Register(txtLogin.Text, txtPassword.Password, nickname)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    txtPassword.Clear();

                    await DialogManager.ShowDialog("Register error", result.message)
                        .ConfigureAwait(true);
                }
                else
                {
                    SettingsManager.PersistentSettings.SetUser(txtLogin.Text,
                        PersistentUtils.WinProtect(result.data.token, $"UserToken-{txtLogin.Text}"),
                        PersistentUtils.WinProtect(result.data.id.ToString(), $"UserId-{txtLogin.Text}"));
                    SettingsManager.PersistentSettings.SetCurrentUserLogin(txtLogin.Text);

                    SettingsManager.PersistentSettings.CurrentUserLogin = txtLogin.Text;
                    SettingsManager.PersistentSettings.CurrentUserToken = result.data.token;
                    SettingsManager.PersistentSettings.CurrentUserId = result.data.id;

                    NavigationController.Instance.RequestPage<FeedPage>();

                    txtLogin.Clear();
                    txtPassword.Clear();
                    txtNickname.Clear();
                    _changeNicknameExplicit = false;
                }
            }
            catch (Exception ex)
            {
                txtPassword.Clear();

                await DialogManager.ShowDialog("An exception happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnRegister.IsEnabled = !NeedBlockRegister();
                //btnAutoRegister.IsEnabled = true;
                btnGoToLogin.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtNickname.IsEnabled = true;
            }
        }

        private async void btnAutoRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;
            //btnAutoRegister.IsEnabled = false;
            btnGoToLogin.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtNickname.IsEnabled = false;

            try
            {
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

                await DialogManager.ShowDialog("S U C C", "Registered user with login: " + name + counter.ToString("D10"))
                    .ConfigureAwait(true);

                SettingsManager.PersistentSettings.SetUser(txtLogin.Text,
                    PersistentUtils.WinProtect(result.data.token, $"UserToken-{txtLogin.Text}"),
                    PersistentUtils.WinProtect(result.data.id.ToString(), $"UserId-{txtLogin.Text}"));
                SettingsManager.PersistentSettings.SetCurrentUserLogin(txtLogin.Text);

                SettingsManager.PersistentSettings.CurrentUserLogin = txtLogin.Text;
                SettingsManager.PersistentSettings.CurrentUserToken = result.data.token;
                SettingsManager.PersistentSettings.CurrentUserId = result.data.id;

                NavigationController.Instance.RequestPage<FeedPage>();

                txtLogin.Clear();
                txtPassword.Clear();
                txtNickname.Clear();
                _changeNicknameExplicit = false;
            }
            catch (Exception ex)
            {
                txtPassword.Clear();

                await DialogManager.ShowDialog("An exception happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnRegister.IsEnabled = !NeedBlockRegister();
                //btnAutoRegister.IsEnabled = true;
                btnGoToLogin.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtNickname.IsEnabled = true;
            }
        }

        private void btnGoToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<LoginPage>();

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
                txtNickname.Focus();
            }
            else if (e.Key == Key.Up)
            {
                txtLogin.Focus();
            }
        }

        private void txtNickname_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (btnRegister.IsEnabled)
                    btnRegister_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Up)
            {
                txtPassword.Focus();
            }
            else
            {
                _changeNicknameExplicit = true;
            }
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnRegister.IsEnabled = !NeedBlockRegister();

            if (!_changeNicknameExplicit)
                txtNickname.Text = txtLogin.Text;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = !NeedBlockRegister();
        }

        private void txtNickname_TextChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
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

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            txtLogin.Clear();
            txtPassword.Clear();
            txtNickname.Clear();
            _changeNicknameExplicit = false;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            base.OnExit(sender, e);
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
                    await DialogManager.ShowDialog("Register error", result.message)
                        .ConfigureAwait(true);
                }
                else
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

                    NavigationController.Instance.RequestPage<FeedPage>();

                    txtLogin.Clear();
                    txtNickname.Clear();
                    _changeNicknameExplicit = false;
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

                btnRegister.IsEnabled = !NeedBlockRegister();
                btnGoToLogin.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtNickname.IsEnabled = true;
            }
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

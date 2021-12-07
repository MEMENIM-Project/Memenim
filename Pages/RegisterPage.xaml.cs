using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Generating;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class RegisterPage : PageContent
    {
        private bool _changeNicknameExplicit;
        private bool _loadingGridShowing;

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

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                _loadingGridShowing = true;
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            _loadingGridShowing = false;
            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (_loadingGridShowing)
                        break;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        loadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    loadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            txtLogin.Clear();
            txtPassword.Clear();
            txtNickname.Clear();
            _changeNicknameExplicit = false;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        private async void btnGeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;
            btnGoToLogin.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtNickname.IsEnabled = false;
            btnGeneratePassword.IsEnabled = false;

            try
            {
                var password = GeneratingManager.RandomStringGenerator
                    .GenerateString(20);

                if (password == null)
                {
                    var message = LocalizationUtils
                        .GetLocalized("CopyingToClipboardErrorMessage");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                txtPassword.Password = password;

                Clipboard.SetText(password);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnRegister.IsEnabled = !NeedBlockRegister();
                btnGoToLogin.IsEnabled = true;
                txtLogin.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtNickname.IsEnabled = true;
                btnGeneratePassword.IsEnabled = true;
            }
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;
            btnGoToLogin.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtNickname.IsEnabled = false;
            btnGeneratePassword.IsEnabled = false;

            try
            {
                string nickname = txtNickname.Text;

                if (string.IsNullOrWhiteSpace(nickname))
                    nickname = null;

                var result = await UserApi.Register(
                        txtLogin.Text, txtPassword.Password, nickname)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    var title = LocalizationUtils.GetLocalized("RegisterErrorTitle");

                    await DialogManager.ShowMessageDialog(title, result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                var setUserSuccess = SettingsManager.PersistentSettings.SetUser(
                    txtLogin.Text,
                    result.Data.Token,
                    result.Data.Id,
                    null);

                if (!setUserSuccess)
                    return;

                if (!SettingsManager.PersistentSettings.SetCurrentUser(
                    txtLogin.Text))
                {
                    return;
                }


                if (NavigationController.Instance.IsEmptyHistory())
                    NavigationController.Instance.RequestPage<FeedPage>();
                else
                    NavigationController.Instance.GoBack();

                txtLogin.Clear();
                txtNickname.Clear();
                _changeNicknameExplicit = false;
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
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
                btnGeneratePassword.IsEnabled = true;
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

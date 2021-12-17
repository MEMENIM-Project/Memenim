using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Extensions;
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

            RegisterButton.IsEnabled = false;
        }



        private bool IsRegisterBlocked()
        {
            return LoginTextBox.Text.Length == 0
                   || PasswordTextBox.Password.Length == 0;
        }



        public Task ShowLoadingGrid()
        {
            _loadingGridShowing = true;

            LoadingIndicator.IsActive = true;
            LoadingGrid.Opacity = 1.0;
            LoadingGrid.IsHitTestVisible = true;
            LoadingGrid.Visibility = Visibility.Visible;

            return Task.CompletedTask;
        }

        public Task HideLoadingGrid()
        {
            _loadingGridShowing = false;

            LoadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (var i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (_loadingGridShowing)
                        break;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LoadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    LoadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }



        protected override void OnFirstEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnFirstEnter(sender, e);

            var popup = LocalizationButton
                .GetPopup();

            popup.Placement = PlacementMode.Top;
        }

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            _changeNicknameExplicit = false;

            LoginTextBox.Clear();
            PasswordTextBox.Clear();
            NicknameTextBox.Clear();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private void LoginTextBox_TextChanged(object sender,
            TextChangedEventArgs e)
        {
            RegisterButton.IsEnabled = !IsRegisterBlocked();

            if (!_changeNicknameExplicit)
                NicknameTextBox.Text = LoginTextBox.Text;
        }

        private void LoginTextBox_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                PasswordTextBox.Focus();
            }
        }

        private void PasswordTextBox_PasswordChanged(object sender,
            RoutedEventArgs e)
        {
            RegisterButton.IsEnabled = !IsRegisterBlocked();
        }

        private void PasswordTextBox_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                NicknameTextBox.Focus();
            }
            else if (e.Key == Key.Up)
            {
                LoginTextBox.Focus();
            }
        }

        private void NicknameTextBox_TextChanged(object sender,
            RoutedEventArgs e)
        {

        }

        private void NicknameTextBox_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (RegisterButton.IsEnabled)
                    RegisterButton_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Up)
            {
                PasswordTextBox.Focus();
            }
            else
            {
                _changeNicknameExplicit = true;
            }
        }

        private async void GeneratePasswordButton_Click(object sender,
            RoutedEventArgs e)
        {
            RegisterButton.IsEnabled = false;
            GoToLoginButton.IsEnabled = false;
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            NicknameTextBox.IsEnabled = false;
            GeneratePasswordButton.IsEnabled = false;

            try
            {
                var password = GeneratingManager
                    .RandomStringGenerator
                    .GenerateString(20);

                if (password == null)
                {
                    var message = LocalizationUtils
                        .GetLocalized("CopyingToClipboardErrorMessage");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                PasswordTextBox.Password = password;

                Clipboard.SetText(password);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                RegisterButton.IsEnabled = !IsRegisterBlocked();
                GoToLoginButton.IsEnabled = true;
                LoginTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                NicknameTextBox.IsEnabled = true;
                GeneratePasswordButton.IsEnabled = true;
            }
        }

        private async void RegisterButton_Click(object sender,
            RoutedEventArgs e)
        {
            RegisterButton.IsEnabled = false;
            GoToLoginButton.IsEnabled = false;
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            NicknameTextBox.IsEnabled = false;
            GeneratePasswordButton.IsEnabled = false;

            try
            {
                var nickname = NicknameTextBox.Text;

                if (string.IsNullOrWhiteSpace(nickname))
                    nickname = null;

                var result = await UserApi.Register(
                        LoginTextBox.Text, PasswordTextBox.Password, nickname)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    var title = LocalizationUtils
                        .GetLocalized("RegisterErrorTitle");

                    await DialogManager.ShowMessageDialog(
                            title, result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                var setUserSuccess = SettingsManager.PersistentSettings.SetUser(
                    LoginTextBox.Text,
                    result.Data.Token,
                    result.Data.Id,
                    null);

                if (!setUserSuccess)
                    return;

                if (!SettingsManager.PersistentSettings.SetCurrentUser(
                    LoginTextBox.Text))
                {
                    return;
                }


                if (NavigationController.Instance.IsEmptyHistory())
                    NavigationController.Instance.RequestPage<FeedPage>();
                else
                    NavigationController.Instance.GoBack();
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                PasswordTextBox.Clear();

                RegisterButton.IsEnabled = !IsRegisterBlocked();
                GoToLoginButton.IsEnabled = true;
                LoginTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                NicknameTextBox.IsEnabled = true;
                GeneratePasswordButton.IsEnabled = true;
            }
        }

        private void GoToLoginButton_Click(object sender,
            RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<LoginPage>();
        }
    }
}

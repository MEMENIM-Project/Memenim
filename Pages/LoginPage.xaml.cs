using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Widgets;

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

            SettingsManager.PersistentSettings.AvailableUsersChanged += OnAvailableUsersChanged;
        }

        ~LoginPage()
        {
            SettingsManager.PersistentSettings.AvailableUsersChanged -= OnAvailableUsersChanged;
        }

        private bool NeedBlockLogin()
        {
            return txtPassword.Password.Length == 0 || txtLogin.Text.Length == 0;
        }

        public async Task ReloadStoredAccounts()
        {
            lstStoredAccounts.Children.Clear();
            svStoredAccounts.ScrollToVerticalOffset(0);

            foreach (var user in SettingsManager.PersistentSettings.AvailableUsers.Values)
            {
                var storedAccountWidget = new StoredAccount
                {
                    Account = user
                };
                storedAccountWidget.AccountClick += StoredAccount_AccountClick;
                storedAccountWidget.AccountDelete += StoredAccount_AccountDelete;

                await storedAccountWidget.UpdateAccount()
                    .ConfigureAwait(true);

                lstStoredAccounts.Children.Add(storedAccountWidget);
            }
        }

        protected override async void OnCreated(object sender, EventArgs e)
        {
            await ReloadStoredAccounts()
                .ConfigureAwait(true);

            base.OnCreated(sender, e);
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            txtLogin.Clear();
            txtPassword.Clear();
            chkRememberMe.IsChecked = false;
            btnOpenStoredAccounts.IsChecked = false;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            base.OnExit(sender, e);
        }

        private async void OnAvailableUsersChanged(object sender, AvailableUsersChangedEventArgs e)
        {
            await ReloadStoredAccounts()
                .ConfigureAwait(true);
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

                        if (!SettingsManager.PersistentSettings.SetCurrentUserTemporary(
                            txtLogin.Text,
                            result.data.token,
                            result.data.id))
                        {
                            return;
                        }
                    }

                    if (NavigationController.Instance.HistoryIsEmpty())
                        NavigationController.Instance.RequestPage<FeedPage>();
                    else
                        NavigationController.Instance.GoBack();

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

        private async void StoredAccount_AccountClick(object sender, RoutedEventArgs e)
        {
            lstStoredAccounts.IsEnabled = false;

            StoredAccount storedAccount = sender as StoredAccount;

            if (storedAccount == null)
                return;

            try
            {
                if (!SettingsManager.PersistentSettings.SetCurrentUser(
                    storedAccount.Account.Login))
                {
                    return;
                }

                NavigationController.Instance.GoBack();

                btnOpenStoredAccounts.IsChecked = false;
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("An exception happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                lstStoredAccounts.IsEnabled = true;
            }
        }

        private void StoredAccount_AccountDelete(object sender, RoutedEventArgs e)
        {
            StoredAccount storedAccount = sender as StoredAccount;

            if (storedAccount == null)
                return;

            lstStoredAccounts.Children.Remove(storedAccount);

            SettingsManager.PersistentSettings.RemoveUser(
                storedAccount.Account.Login);
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

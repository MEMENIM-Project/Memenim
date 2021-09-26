using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
using Timer = System.Timers.Timer;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class LoginPage : PageContent
    {
        private readonly SemaphoreSlim _accountsUpdateLock = new SemaphoreSlim(1, 1);
        private int _accountsUpdateWaitingCount;
        private bool _loadingGridShowing;

        private Timer _autoUpdateStatusTimer;

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
            _autoUpdateStatusTimer.Stop();

            await Dispatcher.Invoke(() =>
            {
                return ShowLoadingGrid(true);
            }).ConfigureAwait(false);

            try
            {
                Interlocked.Increment(ref _accountsUpdateWaitingCount);

                await _accountsUpdateLock.WaitAsync()
                    .ConfigureAwait(true);
            }
            finally
            {
                Interlocked.Decrement(ref _accountsUpdateWaitingCount);
            }

            try
            {
                Dispatcher.Invoke(() =>
                {
                    lstStoredAccounts.Items.Clear();
                });

                if (SettingsManager.PersistentSettings.AvailableUsers.Values.Count == 0)
                    return;

                foreach (var user in SettingsManager.PersistentSettings.AvailableUsers.Values)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var storedAccountWidget = new StoredAccount
                        {
                            Account = user
                        };
                        storedAccountWidget.AccountClick += StoredAccount_AccountClick;
                        storedAccountWidget.AccountDelete += StoredAccount_AccountDelete;

                        lstStoredAccounts.Items.Add(storedAccountWidget);
                    });
                }

                ItemCollection items = null;

                Dispatcher.Invoke(() =>
                {
                    lstStoredAccounts.GetChildOfType<ScrollViewer>()?
                        .ScrollToVerticalOffset(0);

                    items = lstStoredAccounts.Items;
                });

                await Task.Delay(500)
                    .ConfigureAwait(true);

                if (_accountsUpdateWaitingCount == 0)
                {
                    await Dispatcher.Invoke(() =>
                    {
                        return ShowLoadingGrid(false);
                    }).ConfigureAwait(false);
                }

                foreach (var item in items)
                {
                    var storedAccountWidget = item as StoredAccount;

                    if (storedAccountWidget == null)
                        continue;

                    await Dispatcher.Invoke(() =>
                    {
                        return storedAccountWidget.UpdateAccount();
                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                await Task.Delay(500)
                    .ConfigureAwait(true);

                if (_accountsUpdateWaitingCount == 0)
                {
                    await Dispatcher.Invoke(() =>
                    {
                        return ShowLoadingGrid(false);
                    }).ConfigureAwait(false);

                    _autoUpdateStatusTimer.Start();
                }

                _accountsUpdateLock.Release();
            }
        }

        public async Task UpdateStatusAccounts()
        {
            _autoUpdateStatusTimer.Stop();

            await _accountsUpdateLock.WaitAsync()
                .ConfigureAwait(true);

            try
            {
                ItemCollection items = null;

                Dispatcher.Invoke(() =>
                {
                    items = lstStoredAccounts.Items;
                });

                if (items == null
                    || items.Count == 0)
                {
                    return;
                }

                foreach (var item in items)
                {
                    var storedAccountWidget = item as StoredAccount;

                    if (storedAccountWidget == null)
                        continue;

                    await Dispatcher.Invoke(() =>
                    {
                        return storedAccountWidget.UpdateStatus();
                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                if (_accountsUpdateWaitingCount == 0)
                    _autoUpdateStatusTimer.Start();

                _accountsUpdateLock.Release();
            }
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

        protected override async void OnCreated(object sender, EventArgs e)
        {
            base.OnCreated(sender, e);

            DataContext = new LoginViewModel();

            _autoUpdateStatusTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
            _autoUpdateStatusTimer.Elapsed += AutoUpdateStatusTimerCallback;
            _autoUpdateStatusTimer.Stop();

            btnLogin.IsEnabled = false;

            SettingsManager.PersistentSettings.AvailableUsersChanged += OnAvailableUsersChanged;

            await ReloadStoredAccounts()
                .ConfigureAwait(true);
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await UpdateStatusAccounts()
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            _autoUpdateStatusTimer.Stop();

            txtLogin.Clear();
            txtPassword.Clear();
            chkRememberMe.IsChecked = false;
            btnOpenStoredAccounts.IsChecked = false;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        private async void OnAvailableUsersChanged(object sender, AvailableUsersChangedEventArgs e)
        {
            await ReloadStoredAccounts()
                .ConfigureAwait(true);
        }

        private async void AutoUpdateStatusTimerCallback(object sender, ElapsedEventArgs e)
        {
            if (!_autoUpdateStatusTimer.Enabled)
                return;

            if (State != PageStateType.Loaded)
                return;

            await UpdateStatusAccounts()
                .ConfigureAwait(true);
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            btnGoToRegister.IsEnabled = false;
            txtLogin.IsEnabled = false;
            txtPassword.IsEnabled = false;
            chkRememberMe.IsEnabled = false;
            btnOpenStoredAccounts.IsEnabled = false;

            try
            {
                var result = await UserApi.Login(
                        txtLogin.Text, txtPassword.Password)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    var title = LocalizationUtils.GetLocalized("LoginErrorTitle");

                    await DialogManager.ShowMessageDialog(title, result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (chkRememberMe.IsChecked == true)
                {
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
                }
                else
                {
                    SettingsManager.PersistentSettings.RemoveUser(
                        txtLogin.Text);

                    if (!SettingsManager.PersistentSettings.SetCurrentUserTemporary(
                        txtLogin.Text,
                        result.Data.Token,
                        result.Data.Id))
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
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
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
                btnOpenStoredAccounts.IsEnabled = true;
            }
        }

        private void btnGoToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<RegisterPage>();
        }

        private async void StoredAccount_AccountClick(object sender, RoutedEventArgs e)
        {
            lstStoredAccounts.IsEnabled = false;
            btnOpenStoredAccounts.IsEnabled = false;

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

                if (NavigationController.Instance.HistoryIsEmpty())
                    NavigationController.Instance.RequestPage<FeedPage>();
                else
                    NavigationController.Instance.GoBack();

                btnOpenStoredAccounts.IsChecked = false;
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                lstStoredAccounts.IsEnabled = true;
                btnOpenStoredAccounts.IsEnabled = true;
            }
        }

        private void StoredAccount_AccountDelete(object sender, RoutedEventArgs e)
        {
            StoredAccount storedAccount = sender as StoredAccount;

            if (storedAccount == null)
                return;

            lstStoredAccounts.Items.Remove(storedAccount);

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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Extensions;
using Memenim.Generic;
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
        private readonly SemaphoreSlim _accountsUpdateLock;
        private int _accountsUpdateWaitingCount;

        private readonly Timer _autoUpdateStatusesTimer;

        private bool _loadingGridShowing;



        public LoginViewModel ViewModel
        {
            get
            {
                return DataContext as LoginViewModel;
            }
        }



        public LoginPage()
        {
            _accountsUpdateLock =
                new SemaphoreSlim(1, 1);
            _accountsUpdateWaitingCount = 0;

            _autoUpdateStatusesTimer = new Timer
            {
                Interval = TimeSpan
                    .FromSeconds(20)
                    .TotalMilliseconds
            };
            _autoUpdateStatusesTimer.Elapsed += AutoUpdateStatusesTimer_Tick;

            InitializeComponent();
            DataContext = new LoginViewModel();

            LoginButton.IsEnabled = false;

            SettingsManager.PersistentSettings.AvailableUsersChanged += OnAvailableUsersChanged;
        }

        ~LoginPage()
        {
            SettingsManager.PersistentSettings.AvailableUsersChanged -= OnAvailableUsersChanged;
        }



        private bool IsLoginBlocked()
        {
            return LoginTextBox.Text.Length == 0
                   || PasswordTextBox.Password.Length == 0;
        }



        public async Task ReloadStoredAccounts()
        {
            _autoUpdateStatusesTimer.Stop();

            await Dispatcher.Invoke(() =>
            {
                return ShowLoadingGrid();
            }).ConfigureAwait(false);

            try
            {
                Interlocked.Increment(
                    ref _accountsUpdateWaitingCount);

                await _accountsUpdateLock.WaitAsync()
                    .ConfigureAwait(true);
            }
            finally
            {
                Interlocked.Decrement(
                    ref _accountsUpdateWaitingCount);
            }

            try
            {
                Dispatcher.Invoke(() =>
                {
                    StoredAccountsListBox.Items.Clear();
                });

                if (SettingsManager.PersistentSettings.AvailableUsers.Values.Count == 0)
                    return;

                foreach (var user in SettingsManager.PersistentSettings.AvailableUsers.Values)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var storedAccount = new StoredAccount
                        {
                            UserAccount = user,
                            Margin = new Thickness(5)
                        };
                        storedAccount.Click += StoredAccount_Click;
                        storedAccount.AccountDelete += StoredAccount_AccountDelete;

                        StoredAccountsListBox.Items.Add(
                            storedAccount);
                    });
                }

                ItemCollection items = null;

                Dispatcher.Invoke(() =>
                {
                    StoredAccountsListBox
                        .GetChildOfType<ScrollViewer>()?
                        .ScrollToVerticalOffset(0);

                    items = StoredAccountsListBox.Items;
                });

                await Task.Delay(
                        TimeSpan.FromSeconds(0.5))
                    .ConfigureAwait(true);

                if (_accountsUpdateWaitingCount == 0)
                {
                    await Dispatcher.Invoke(() =>
                    {
                        return HideLoadingGrid();
                    }).ConfigureAwait(false);
                }

                foreach (var item in items)
                {
                    if (!(item is StoredAccount storedAccount))
                        continue;

                    await Dispatcher.Invoke(() =>
                    {
                        return storedAccount
                            .UpdateAccount();
                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                await Task.Delay(
                        TimeSpan.FromSeconds(0.5))
                    .ConfigureAwait(true);

                if (_accountsUpdateWaitingCount == 0)
                {
                    await Dispatcher.Invoke(() =>
                    {
                        return HideLoadingGrid();
                    }).ConfigureAwait(false);

                    _autoUpdateStatusesTimer.Start();
                }

                _accountsUpdateLock.Release();
            }
        }

        public async Task UpdateAccountStatuses()
        {
            _autoUpdateStatusesTimer.Stop();

            await _accountsUpdateLock.WaitAsync()
                .ConfigureAwait(true);

            try
            {
                ItemCollection items = null;

                Dispatcher.Invoke(() =>
                {
                    items = StoredAccountsListBox.Items;
                });

                if (items == null
                    || items.Count == 0)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (!(item is StoredAccount storedAccountWidget))
                        continue;

                    await Dispatcher.Invoke(() =>
                    {
                        return storedAccountWidget
                            .UpdateStatus();
                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                if (_accountsUpdateWaitingCount == 0)
                    _autoUpdateStatusesTimer.Start();

                _accountsUpdateLock.Release();
            }
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



        protected override async void OnCreated(object sender,
            EventArgs e)
        {
            base.OnCreated(sender, e);

            await ReloadStoredAccounts()
                .ConfigureAwait(true);
        }

        protected override void OnFirstEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnFirstEnter(sender, e);

            var popup = LocalizationButton
                .GetPopup();

            popup.Placement = PlacementMode.Top;
        }

        protected override async void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await UpdateAccountStatuses()
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            _autoUpdateStatusesTimer.Stop();

            LoginTextBox.Clear();
            PasswordTextBox.Clear();

            RememberMeCheckBox.IsChecked = false;
            OpenStoredAccountsButton.IsChecked = false;

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private async void OnAvailableUsersChanged(object sender,
            AvailableUsersChangedEventArgs e)
        {
            await ReloadStoredAccounts()
                .ConfigureAwait(true);
        }



        private void LoginTextBox_TextChanged(object sender,
            TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = !IsLoginBlocked();
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
            LoginButton.IsEnabled = !IsLoginBlocked();
        }

        private void PasswordTextBox_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Down)
            {
                if (LoginButton.IsEnabled)
                    LoginButton_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Up)
            {
                LoginTextBox.Focus();
            }
        }

        private async void LoginButton_Click(object sender,
            RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            GoToRegisterButton.IsEnabled = false;
            LoginTextBox.IsEnabled = false;
            PasswordTextBox.IsEnabled = false;
            RememberMeCheckBox.IsEnabled = false;
            OpenStoredAccountsButton.IsEnabled = false;

            try
            {
                var result = await UserApi.Login(
                        LoginTextBox.Text, PasswordTextBox.Password)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    var title = LocalizationUtils
                        .GetLocalized("LoginErrorTitle");

                    await DialogManager.ShowMessageDialog(
                            title, result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (RememberMeCheckBox.IsChecked == true)
                {
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
                }
                else
                {
                    SettingsManager.PersistentSettings.RemoveUser(
                        LoginTextBox.Text);

                    if (!SettingsManager.PersistentSettings.SetCurrentUserTemporary(
                        LoginTextBox.Text,
                        result.Data.Token,
                        result.Data.Id))
                    {
                        return;
                    }
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

                LoginButton.IsEnabled = !IsLoginBlocked();
                GoToRegisterButton.IsEnabled = true;
                LoginTextBox.IsEnabled = true;
                PasswordTextBox.IsEnabled = true;
                RememberMeCheckBox.IsEnabled = true;
                OpenStoredAccountsButton.IsEnabled = true;
            }
        }

        private void GoToRegisterButton_Click(object sender,
            RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<RegisterPage>();
        }



        private async void StoredAccount_Click(object sender,
            RoutedEventArgs e)
        {
            StoredAccountsListBox.IsEnabled = false;
            OpenStoredAccountsButton.IsEnabled = false;

            if (!(sender is StoredAccount storedAccount))
                return;

            try
            {
                if (!SettingsManager.PersistentSettings.SetCurrentUser(
                        storedAccount.UserAccount.Login))
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
                StoredAccountsListBox.IsEnabled = true;
                OpenStoredAccountsButton.IsEnabled = true;
            }
        }

        private void StoredAccount_AccountDelete(object sender,
            RoutedEventArgs e)
        {
            if (!(sender is StoredAccount storedAccount))
                return;

            StoredAccountsListBox.Items.Remove(
                storedAccount);

            SettingsManager.PersistentSettings.RemoveUser(
                storedAccount.UserAccount.Login);
        }



        private async void AutoUpdateStatusesTimer_Tick(object sender,
            ElapsedEventArgs e)
        {
            if (!_autoUpdateStatusesTimer.Enabled)
                return;

            if (State != ControlStateType.Loaded)
                return;

            await UpdateAccountStatuses()
                .ConfigureAwait(true);
        }
    }
}

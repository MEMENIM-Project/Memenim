using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Pages.ViewModel;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class UserProfilePage : PageContent
    {
        private readonly SemaphoreSlim _profileUpdateLock = new SemaphoreSlim(1, 1);
        private int _profileUpdateWaitingCount;
        private bool _loadingGridShowing;

        public UserProfileViewModel ViewModel
        {
            get
            {
                return DataContext as UserProfileViewModel;
            }
        }

        public UserProfilePage()
        {
            InitializeComponent();
            DataContext = new UserProfileViewModel();
        }

        public Task UpdateProfile()
        {
            return UpdateProfile(ViewModel.CurrentProfileData.id);
        }

        public async Task UpdateProfile(int id)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            try
            {
                Interlocked.Increment(ref _profileUpdateWaitingCount);

                await _profileUpdateLock.WaitAsync()
                    .ConfigureAwait(true);
            }
            finally
            {
                Interlocked.Decrement(ref _profileUpdateWaitingCount);
            }

            try
            {
                if (id == -1)
                    return;

                var result = await UserApi.GetProfileById(id)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                    return;
                }

                if (result.data == null)
                    return;

                ViewModel.CurrentProfileData = result.data;
            }
            finally
            {
                await Task.Delay(500)
                    .ConfigureAwait(true);

                if (_profileUpdateWaitingCount == 0)
                {
                    await ShowLoadingGrid(false)
                        .ConfigureAwait(true);
                }

                _profileUpdateLock.Release();
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

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            await UpdateProfile()
                .ConfigureAwait(true);
        }

        protected override async void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ViewModelPropertyChanged(sender, e);

            if (e.PropertyName.Length == 0)
            {
                await UpdateProfile()
                    .ConfigureAwait(true);
            }
        }
    }
}

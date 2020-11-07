using System.Threading.Tasks;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;

namespace Memenim.Pages
{
    public partial class UserProfilePage : PageContent
    {
        public static readonly DependencyProperty CurrentProfileDataProperty =
            DependencyProperty.Register(nameof(CurrentProfileData), typeof(ProfileSchema), typeof(UserProfilePage),
                new PropertyMetadata(new ProfileSchema { id = -1 }));

        public ProfileSchema CurrentProfileData
        {
            get
            {
                return (ProfileSchema)GetValue(CurrentProfileDataProperty);
            }
            set
            {
                SetValue(CurrentProfileDataProperty, value);
            }
        }

        public UserProfilePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Task UpdateProfile()
        {
            return UpdateProfile(CurrentProfileData.id);
        }
        public async Task UpdateProfile(int id)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            if (id == -1)
            {
                CurrentProfileData = new ProfileSchema
                {
                    name = "Unknown"
                };
                wpStats.Visibility = Visibility.Hidden;

                return;
            }

            var result = await UserApi.GetProfileById(id)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            if (result.data == null)
            {
                CurrentProfileData = new ProfileSchema
                {
                    name = "Unknown"
                };
                wpStats.Visibility = Visibility.Hidden;

                return;
            }

            CurrentProfileData = result.data;

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);
        }

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (opacity < 0.7)
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
            base.OnEnter(sender, e);

            await UpdateProfile()
                .ConfigureAwait(true);
        }
    }
}

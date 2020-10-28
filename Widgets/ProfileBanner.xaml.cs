using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Utils;

namespace Memenim.Widgets
{
    public partial class ProfileBanner : UserControl
    {
        public static readonly DependencyProperty CurrentProfileDataProperty =
            DependencyProperty.Register("CurrentProfileData", typeof(ProfileData), typeof(ProfileBanner),
                new PropertyMetadata(new ProfileData { id = -1 }));

        public ProfileData CurrentProfileData
        {
            get
            {
                return (ProfileData)GetValue(CurrentProfileDataProperty);
            }
            set
            {
                SetValue(CurrentProfileDataProperty, value);
            }
        }

        public ProfileBanner()
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
            if (id == -1)
            {
                CurrentProfileData = new ProfileData
                {
                    name = "Unknown"
                };

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

            CurrentProfileData = result.data[0];
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateProfile()
                .ConfigureAwait(true);
        }

        private void SelectAvatarFromTennor_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<TennorSearchPage>(new TennorSearchPage
            {
                OnPicSelect = ProfileUtils.ChangeAvatar
            });
        }

        private async void SelectAvatarFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                .ConfigureAwait(true);

            await ProfileUtils.ChangeAvatar(url)
                .ConfigureAwait(true);
        }

        private void SelectBannerFromTennor_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<TennorSearchPage>(new TennorSearchPage
            {
                OnPicSelect = ProfileUtils.ChangeBanner
            });
        }

        private async void SelectBannerFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                .ConfigureAwait(true);

            await ProfileUtils.ChangeBanner(url)
                .ConfigureAwait(true);
        }
    }
}

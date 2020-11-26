using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Utils;

namespace Memenim.Widgets
{
    public partial class ProfileBanner : UserControl
    {
        public static readonly DependencyProperty CurrentProfileDataProperty =
            DependencyProperty.Register(nameof(CurrentProfileData), typeof(ProfileSchema), typeof(ProfileBanner),
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
                CurrentProfileData = new ProfileSchema
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

            if (result.data == null)
            {
                CurrentProfileData = new ProfileSchema
                {
                    name = "Unknown"
                };

                return;
            }

            CurrentProfileData = result.data;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateProfile()
                .ConfigureAwait(true);
        }

        private async void SelectAvatarFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(url))
                return;

            await ProfileUtils.ChangeAvatar(url)
                .ConfigureAwait(true);

            //CurrentProfileData.photo = url;

            await UpdateProfile()
                .ConfigureAwait(true);
        }

        private void SelectAvatarFromAnonymGallery_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
            {
                OnPicSelect = async url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    await ProfileUtils.ChangeAvatar(url)
                        .ConfigureAwait(true);

                    //Dispatcher.Invoke(() =>
                    //{
                    //    CurrentProfileData.photo = url;
                    //});

                    await UpdateProfile()
                        .ConfigureAwait(true);
                }
            });
        }

        private void SelectAvatarFromTenor_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
            {
                OnPicSelect = async url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    await ProfileUtils.ChangeAvatar(url)
                        .ConfigureAwait(true);

                    //Dispatcher.Invoke(() =>
                    //{
                    //    CurrentProfileData.photo = url;
                    //});

                    await UpdateProfile()
                        .ConfigureAwait(true);
                }
            });
        }

        private async void RemoveAvatar_Click(object sender, RoutedEventArgs e)
        {
            await ProfileUtils.RemoveAvatar()
                .ConfigureAwait(true);

            //Dispatcher.Invoke(() =>
            //{
            //    CurrentProfileData.photo = string.Empty;
            //});

            await UpdateProfile()
                .ConfigureAwait(true);
        }

        private async void SelectBannerFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(url))
                return;

            await ProfileUtils.ChangeBanner(url)
                .ConfigureAwait(true);

            //CurrentProfileData.banner = url;

            await UpdateProfile()
                .ConfigureAwait(true);
        }

        private void SelectBannerFromAnonymGallery_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
            {
                OnPicSelect = async url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    await ProfileUtils.ChangeBanner(url)
                        .ConfigureAwait(true);

                    //Dispatcher.Invoke(() =>
                    //{
                    //    CurrentProfileData.banner = url;
                    //});

                    await UpdateProfile()
                        .ConfigureAwait(true);
                }
            });
        }

        private void SelectBannerFromTenor_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
            {
                OnPicSelect = async url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    await ProfileUtils.ChangeBanner(url)
                        .ConfigureAwait(true);

                    //Dispatcher.Invoke(() =>
                    //{
                    //    CurrentProfileData.banner = url;
                    //});

                    await UpdateProfile()
                        .ConfigureAwait(true);
                }
            });
        }

        private async void RemoveBanner_Click(object sender, RoutedEventArgs e)
        {
            await ProfileUtils.RemoveBanner()
                .ConfigureAwait(true);

            //Dispatcher.Invoke(() =>
            //{
            //    CurrentProfileData.banner = string.Empty;
            //});

            await UpdateProfile()
                .ConfigureAwait(true);
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Dialogs;

namespace Memenim.Pages
{
    public partial class UserProfilePage : UserControl
    {
        public static readonly DependencyProperty CurrentProfileDataProperty =
            DependencyProperty.Register("CurrentProfileData", typeof(ProfileData), typeof(UserProfilePage),
                new PropertyMetadata(new ProfileData {id = -1}));

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
            if (id == -1)
            {
                CurrentProfileData = new ProfileData
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

            CurrentProfileData = result.data[0];
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateProfile()
                .ConfigureAwait(true);
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : UserControl
    {
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register("CurrentPostData", typeof(PostData), typeof(PostOverlayPage), new PropertyMetadata((PostData)null));

        public PostData CurrentPostData
        {
            get
            {
                return (PostData)GetValue(CurrentPostDataProperty);
            }
            set
            {
                SetValue(CurrentPostDataProperty, value);
            }
        }

        public PostOverlayPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var result = await UserApi.GetProfileById(SettingManager.PersistentSettings.CurrentUserId)
                .ConfigureAwait(true);

            wdgUserComment.UserAvatarSource = result.data[0].photo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.CloseOverlay();
        }
    }
}

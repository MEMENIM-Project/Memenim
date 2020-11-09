using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Pages.ViewModel;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : PageContent
    {
        public PostOverlayViewModel ViewModel
        {
            get
            {
                return DataContext as PostOverlayViewModel;
            }
        }

        public PostOverlayPage()
        {
            InitializeComponent();
            DataContext = new PostOverlayViewModel();
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            var result = await UserApi.GetProfileById(SettingsManager.PersistentSettings.CurrentUserId)
                .ConfigureAwait(true);

            if (result.data == null)
                return;

            wdgUserComment.UserAvatarSource = result.data.photo;

            svPost.ScrollToVerticalOffset(0.0);
        }

        private void SvPost_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.VerticalOffset;
        }
    }
}

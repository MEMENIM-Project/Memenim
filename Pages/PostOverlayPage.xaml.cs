using System;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : PageContent
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

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            var result = await UserApi.GetProfileById(SettingsManager.PersistentSettings.CurrentUserId)
                .ConfigureAwait(true);

            wdgUserComment.UserAvatarSource = result.data[0].photo;
        }
    }
}

using System;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : PageContent
    {
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register(nameof(CurrentPostData), typeof(PostSchema), typeof(PostOverlayPage),
                new PropertyMetadata((PostSchema) null));

        public PostSchema CurrentPostData
        {
            get
            {
                return (PostSchema)GetValue(CurrentPostDataProperty);
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

            if (result.data == null)
                return;

            wdgUserComment.UserAvatarSource = result.data.photo;
        }
    }
}

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Memenim.Utils;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Widgets
{
    public partial class PostWidget : UserControl
    {
        public static readonly RoutedEvent OnPostClicked = EventManager.RegisterRoutedEvent("OnPostClick", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(PostWidget));
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register("CurrentPostData", typeof(PostData), typeof(PostWidget), new PropertyMetadata((PostData) null));

        public event EventHandler<RoutedEventArgs> PostClick
        {
            add
            {
                AddHandler(OnPostClicked, value);
            }
            remove
            {
                RemoveHandler(OnPostClicked, value);
            }
        }

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
        public bool PreviewMode { get; set; }

        public PostWidget()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async Task UpdatePost()
        {
            var res = await PostApi.GetById(CurrentPostData.id, SettingManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (res.error)
            {
                await DialogManager.ShowDialog("F U C K", res.message)
                    .ConfigureAwait(true);
                return;
            }

            CurrentPostData = res.data[0];
        }

        public void ShowImage()
        {
            imgPost.Source = CurrentPostData?.attachments?[0]?.photo?.photo_medium != null
                             && Uri.TryCreate(CurrentPostData.attachments[0].photo.photo_medium, UriKind.Absolute, out Uri uri)
                ? new BitmapImage(uri)
                : null;
        }

        public void HideImage()
        {
            imgPost.Source = null;
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            CurrentPostData.attachments[0].photo.photo_medium = url;

            ShowImage();
        }

        public void ClearImage()
        {
            CurrentPostData.attachments[0].photo.photo_medium = string.Empty;

            HideImage();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            wdgPoster.PostTime = TimeUtils.UnixTimeStampToDateTime(CurrentPostData?.date ?? 0L).ToString(CultureInfo.CurrentCulture);
            wdgPoster.IsAnonymous = CurrentPostData?.author_watch != 2;

            if (CurrentPostData?.owner_id.HasValue == true && CurrentPostData?.owner_id != -1)
            {
                var result = await UserApi.GetProfileById(CurrentPostData.owner_id.Value)
                    .ConfigureAwait(true);

                CurrentPostData.owner_name = result.data[0].name;
                wdgPoster.UserName = result.data[0].name;
                wdgPoster.UserAvatarSource = result.data[0].photo;
            }

            if (PreviewMode)
            {
                wdgPoster.PostTime = (CurrentPostData?.date ?? 0L) == 0L
                    ? DateTime.UtcNow.ToLocalTime().ToString(CultureInfo.CurrentCulture)
                    : wdgPoster.PostTime;
                wdgPoster.IsAnonymous = CurrentPostData?.author_watch != 1;

                stLikes.IsEnabled = false;
                stDislikes.IsEnabled = false;
                stComments.IsEnabled = false;
                stShare.IsEnabled = false;
            }
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnPostClicked));
        }

        private void CopyPostID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            var result = await PostApi.AddLike(CurrentPostData.id, SettingManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            await UpdatePost()
                .ConfigureAwait(true);
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            var result = await PostApi.AddDislike(CurrentPostData.id, SettingManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            await UpdatePost()
                .ConfigureAwait(true);
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Memenim.Utils;
using Memenim.Core.Api;
using Memenim.Core.Data;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for PostWidget.xaml
    /// </summary>
    public partial class PostWidget : UserControl
    {
        public string ImageURL { get; set; }
        public string PostText { get; set; }
        public string PostComments { get; set; }
        public string PostShares { get; set; }
        public string PostLikes { get; set; }
        public string PostDislikes { get; set; }
        public PostData CurrentPostData { get; set; }
        public bool PreviewMode { get; set; } = false;

        public static readonly RoutedEvent OnPostClicked = EventManager.RegisterRoutedEvent("OnPostClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(PostWidget));

        // expose our event
        public event RoutedEventHandler PostClick
        {
            add { AddHandler(OnPostClicked, value); }
            remove { RemoveHandler(OnPostClicked, value); }
        }


        public PostWidget()
        {
            InitializeComponent();
        }

        private void ViewPost_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OnPostClicked);
            RaiseEvent(newEventArgs);
        }

        private void CopyPostID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = CurrentPostData;

            wdgPoster.IsAnonymousPost = CurrentPostData.author_watch > 1;
            wdgPoster.PosterID = CurrentPostData.owner_id.GetValueOrDefault(-1);
            wdgPoster.PosterName = CurrentPostData.owner_name;
            wdgPoster.PostTime = TimeUtils.UnixTimeStampToDateTime(CurrentPostData.date).ToString();

            if (!PreviewMode)
            {
                stLikes.StatValue = CurrentPostData.likes.count.ToString();
                stDislikes.StatValue = CurrentPostData.dislikes.count.ToString();
                stComments.StatValue = CurrentPostData.comments.count.ToString();
                stShare.StatValue = CurrentPostData.reposts.ToString();
            }
            else
            {
                stLikes.IsEnabled = false;
                stDislikes.IsEnabled = false;
            }
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostApi.AddLike(CurrentPostData.id, AppPersistent.UserToken);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            await UpdatePostStats();
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostApi.AddDislike(CurrentPostData.id, AppPersistent.UserToken);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            await UpdatePostStats();
        }

        async Task UpdatePostStats()
        {
            var res = await PostApi.GetById(CurrentPostData.id, AppPersistent.UserToken);

            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }

            CurrentPostData = res.data[0];

            stLikes.StatValue = CurrentPostData.likes.count.ToString();
            stDislikes.StatValue = CurrentPostData.dislikes.count.ToString();
            stComments.StatValue = CurrentPostData.comments.count.ToString();
            stShare.StatValue = CurrentPostData.reposts.ToString();
        }


        public void ReloadImage()
        {
            imgPost.Source = new BitmapImage(new Uri(CurrentPostData.attachments[0].photo.photo_medium));
        }
    }
}

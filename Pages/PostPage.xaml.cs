using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Memenim.Utils;
using Memenim.Widgets;
using Memenim.Core.Api;
using Memenim.Core.Data;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for Posts.xaml
    /// </summary>
    public partial class PostPage : UserControl
    {
        private int? m_PosterId;

        DispatcherTimer m_UpdateTimer;

        PostData m_PostData;

        public PostPage()
        {
            InitializeComponent();
            m_UpdateTimer = new DispatcherTimer();
            m_UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            m_UpdateTimer.Interval = new TimeSpan(0, 0, 30);
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var res = await PostApi.GetById(GeneralBlackboard.TryGetValue<int>(BlackBoardValues.EPostData), AppPersistent.UserToken);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
            }

            m_PostData = res.data[0];

            if (m_PostData != null)
            {
                UpdateComments(m_PostData.id);
            }
        }

        private async void PostPage_Loaded(object sender, RoutedEventArgs e)
        {
            var res = await PostApi.GetById(GeneralBlackboard.TryGetValue<int>(BlackBoardValues.EPostData), AppPersistent.UserToken);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
            }

            m_PostData = res.data[0];

            if (m_PostData != null)
            {
                wdgComment.PostID = m_PostData.id;
                m_PosterId = m_PostData.owner_id;
                //wdgPost.CurrentPostData = post;
                //wdgPost.PostText = post.text;
                //wdgPost.ImageURL = post.attachments[0].photo.photo_medium;
                //txtPost.Text = m_PostData.text;
                //imgPost.Source = new BitmapImage(new Uri(m_PostData.attachments[0].photo.photo_medium, UriKind.Absolute));
                lblPosterName.Content = m_PostData.owner_name;
                //lblPosterName.IsEnabled = post.author_watch == 1 ? false : true;
                lblDate.Content = TimeUtils.UnixTimeStampToDateTime(m_PostData.date).ToString();
                UpdateComments(m_PostData.id);
            }
            m_UpdateTimer.Start();
        }

        private async Task UpdateComments(int id)
        {
            var commentsData = await PostApi.GetComments(id);
            if (commentsData.data.Count == 0) { return; }
            lstComments.Items.Clear();
            for (int i = commentsData.data.Count - 1; i > -1; --i)
            {
                UserComment commentWidget = new UserComment();
                commentWidget.UserName = commentsData.data[i].user.name;
                commentWidget.Comment = commentsData.data[i].text;
                commentWidget.ImageURL = commentsData.data[i].user.photo;
                commentWidget.UserID = commentsData.data[i].user.id;
                commentWidget.CommentID = commentsData.data[i].id;
                lstComments.Items.Add(commentWidget);
            }

        }


        private async void ViewUserProfile_Click(object sender, RoutedEventArgs e)
        {
            int id = (lstComments.SelectedItem as UserComment).UserID;
            var profile = await UserApi.GetProfileById(id);
            if (profile.error)
            {
                DialogManager.ShowDialog("ERROR", profile.message);
                return;
            }
            GoToProfile(profile.data[0]);
        }


        private void CopyUserId_Click(object sender, RoutedEventArgs e)
        {
            UserComment comment = (UserComment)lstComments.SelectedItem;
            Clipboard.SetText(comment.UserID.ToString());
        }

        private async void GoToPosterProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = await UserApi.GetProfileById(m_PosterId.GetValueOrDefault());
            GoToProfile(profile.data[0]);
        }

        private void GoToProfile(ProfileData data)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EProfileData, data.id);
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            //PageNavigationManager.SwitchToSubpage(new UserProfilePage());
        }
    }
}

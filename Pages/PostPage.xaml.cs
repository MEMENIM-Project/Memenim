
using AnonymDesktopClient.Core;
using AnonymDesktopClient.Pages;
using Memenim.Core;
using Memenim.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AnonymDesktopClient
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
            var res = await PostAPI.GetPostById(GeneralBlackboard.TryGetValue<int>(BlackBoardValues.EPostData), AppPersistent.UserToken);
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
            var res = await PostAPI.GetPostById(GeneralBlackboard.TryGetValue<int>(BlackBoardValues.EPostData), AppPersistent.UserToken);
            if(res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
            }

            m_PostData = res.data[0];

            if(m_PostData != null)
            {
                wdgComment.PostID = m_PostData.id;
                m_PosterId = m_PostData.owner_id;
                //wdgPost.CurrentPostData = post;
                //wdgPost.PostText = post.text;
                //wdgPost.ImageURL = post.attachments[0].photo.photo_medium;
                txtPost.Text = m_PostData.text;
                imgPost.Source = new BitmapImage(new Uri(m_PostData.attachments[0].photo.photo_medium, UriKind.Absolute));
                lblPosterName.Content = m_PostData.owner_name;
                //lblPosterName.IsEnabled = post.author_watch == 1 ? false : true;
                lblDate.Content = Utils.UnixTimeStampToDateTime(m_PostData.date).ToString();
                UpdateComments(m_PostData.id);
            }
            m_UpdateTimer.Start();
        }

        async void UpdateComments(int id)
        {
            var commentsData = await PostAPI.GetCommentsForPost(id);
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
            var profile = await UsersAPI.GetUserProfileByID(id);
            if(profile.error)
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
            var profile = await UsersAPI.GetUserProfileByID(m_PosterId.GetValueOrDefault());
            GoToProfile(profile.data[0]);
        }

        private void GoToProfile(ProfileData data)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EProfileData, data.id);
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubpage(new UserProfilePage());
        }
    }
}

using AnonymDesktopClient.DataStructs;
using AnonymDesktopClient.Pages;
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

        public PostPage()
        {
            InitializeComponent();
            m_UpdateTimer = new DispatcherTimer();
            m_UpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            m_UpdateTimer.Interval = new TimeSpan(0, 0, 30);
            m_UpdateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            PostData post = GeneralBlackboard.TryGetValue<PostData>(BlackBoardValues.EPostData);
            if(post != null)
            {
                UpdateComments(post.id);
            }
        }

        private void PostPage_Loaded(object sender, RoutedEventArgs e)
        {
            PostData post = GeneralBlackboard.TryGetValue<PostData>(BlackBoardValues.EPostData);
            if(post != null)
            {
                wdgComment.PostID = post.id;
                m_PosterId = post.owner_id;
                //wdgPost.CurrentPostData = post;
                //wdgPost.PostText = post.text;
                //wdgPost.ImageURL = post.attachments[0].photo.photo_medium;
                txtPost.Text = post.text;
                imgPost.Source = new BitmapImage(new Uri(post.attachments[0].photo.photo_medium, UriKind.Absolute));
                lblPosterName.Content = post.owner_name;
                //lblPosterName.IsEnabled = post.author_watch == 1 ? false : true;
                lblDate.Content = UnixTimeStampToDateTime(post.date).ToString();
                UpdateComments(post.id);
            }
        }

        async void UpdateComments(int id)
        {
            var commentsData = await ApiHelper.GetCommentsForPost(id);
            if (commentsData.Count == 0) { return; }
            lstComments.Items.Clear();
            for (int i = commentsData.Count - 1; i > 0; --i)
            {
                UserComment commentWidget = new UserComment();
                commentWidget.UserName = commentsData[i].user.name;
                commentWidget.Comment = commentsData[i].text;
                commentWidget.ImageURL = commentsData[i].user.photo;
                commentWidget.UserID = commentsData[i].user.id;
                commentWidget.CommentID = commentsData[i].id;
                lstComments.Items.Add(commentWidget);
            }

        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private async void ViewUserProfile_Click(object sender, RoutedEventArgs e)
        {
            int id = (lstComments.SelectedItem as UserComment).UserID;
            ProfileData profile = await ApiHelper.GetUserInfo(id);
            GeneralBlackboard.SetValue(BlackBoardValues.EProfileData, profile);
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubpage(new UserProfilePage());
        }


        private void CopyUserId_Click(object sender, RoutedEventArgs e)
        {
            UserComment comment = (UserComment)lstComments.SelectedItem;
            Clipboard.SetText(comment.UserID.ToString());
        }

        private async void GoToPosterProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileData profile = await ApiHelper.GetUserInfo(m_PosterId.GetValueOrDefault());
            GeneralBlackboard.SetValue(BlackBoardValues.EProfileData, profile);
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubpage(new UserProfilePage());
        }
    }
}

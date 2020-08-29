using AnonymDesktopClient.DataStructs;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Data;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for PostsPage.xaml
    /// </summary>
    public partial class PostsPage : UserControl
    {
        public PostsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var posts = await ApiHelper.GetAllPosts();

            if(posts != null)
            {
                lstPostsView.ItemsSource = posts;
                lstPostsView.DisplayMemberPath = "text";
            }
        }

        private async void lstPostsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PostData post = (PostData)lstPostsView.SelectedItem;
            if(post != null)
            {
                wdgComment.PostID = post.id;
                lblPosterName.Content = post.owner_name;
                lblPosterName.IsEnabled = post.hidden == 0 ? true : false;
                lblDate.Content = new DateTime(post.date).ToLocalTime().ToString();
                var commentsData = await ApiHelper.GetCommentsForPost(post.id);
                lstComments.Items.Clear();
                for (int i = commentsData.Count - 1; i > 0; --i)
                {
                    UserComment commentWidget = new UserComment();
                    commentWidget.UserName = commentsData[i].user.name;
                    commentWidget.Comment = commentsData[i].text;
                    commentWidget.ImageURL = commentsData[i].user.photo;
                    commentWidget.UserID = commentsData[i].user.id;
                    lstComments.Items.Add(commentWidget);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UserComment comment = (UserComment)lstComments.SelectedItem;
            Clipboard.SetText(comment.UserID.ToString());
        }

        private void btnCopyPostId_Click(object sender, RoutedEventArgs e)
        {
            PostData post = (PostData)lstPostsView.SelectedItem;
            Clipboard.SetText(post.id.ToString());
        }
    }
}

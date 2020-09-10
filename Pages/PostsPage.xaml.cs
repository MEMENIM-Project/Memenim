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
using AnonymDesktopClient.Pages;
using AnonymDesktopClient.Widgets;

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
            loadingRing.Visibility = Visibility.Visible;
            var posts = await ApiHelper.GetAllPosts();

            UpdatePosts(posts);
            loadingRing.Visibility = Visibility.Hidden;
        }

        bool UpdatePosts(List<PostData> posts)
        {
            if (posts == null) { return false; }
            postsPanel.Items.Clear();
            foreach (var post in posts)
            {
                PostWidget widget = new PostWidget();
                widget.PostText = post.text;
                widget.ImageURL = post.attachments[0].photo.photo_medium;
                widget.CurrentPostData = post;
                widget.PostLikes = post.likes.count.ToString();
                widget.PostDislikes = post.dislikes.count.ToString();
                widget.PostComments = post.comments.count.ToString();
                widget.PostShares = post.reposts.ToString();
                postsPanel.Items.Add(widget);
            }

            return true;
        }
    }
}

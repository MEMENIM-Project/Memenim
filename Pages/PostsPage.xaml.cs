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
using System.Web;
using System.Windows.Controls.Primitives;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for PostsPage.xaml
    /// </summary>
    public partial class PostsPage : UserControl
    {

        public List<PostRequest.EPostType> PostTypes { get; } = new List<PostRequest.EPostType>()
        { PostRequest.EPostType.Popular, PostRequest.EPostType.New, PostRequest.EPostType.My, PostRequest.EPostType.Favorite };

        private int m_PostsCount = 20;

        public PostsPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            loadingRing.Visibility = Visibility.Visible;

            PostRequest request = new PostRequest();
            request.count = m_PostsCount;
            await UpdatePosts(request);
            loadingRing.Visibility = Visibility.Hidden;
        }

        async Task<bool> UpdatePosts(PostRequest filters)
        {
            var posts = await ApiHelper.GetAllPosts(filters);

            if (posts == null) { return false; }
            postsPanel.Items.Clear();
            foreach (var post in posts)
            {
                PostWidget widget = new PostWidget();
                widget.PostText = post.text;
                widget.ImageURL = post.attachments[0].photo.photo_medium;
                widget.CurrentPostData = post;
                postsPanel.Items.Add(widget);
            }

            return true;
        }

        private async void lstPostType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadingRing.Visibility = Visibility.Visible;

            PostRequest request = new PostRequest();
            request.type = (PostRequest.EPostType)((sender as Selector).SelectedItem);
            request.count = m_PostsCount;
            await UpdatePosts(request);
            loadingRing.Visibility = Visibility.Hidden;

        }

        private async void ScrollEnd()
        {
            MessageBox.Show("This is the end");
        }

        //private void postsPanel_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    //if (scrollViewer.HorizontalOffset == scrollViewer.Width)
        //    //{
        //    //    
        //    //}
        //}
    }
}

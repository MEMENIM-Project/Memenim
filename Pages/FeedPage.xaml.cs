
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
using Memenim.Core;
using Memenim.Core.Data;
using AnonymDesktopClient.Core;
using AnonymDesktopClient.Core.Utils;
using Microsoft.Win32;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for PostsPage.xaml
    /// </summary>
    public partial class FeedPage : UserControl
    {
        public ICommand OnPostScrollEnd { get; set; }

        public List<PostRequest.EPostType> PostTypes { get; } = new List<PostRequest.EPostType>()
        { PostRequest.EPostType.Popular, PostRequest.EPostType.New, PostRequest.EPostType.My, PostRequest.EPostType.Favorite };

        private int m_PostsCount = 20;
        private int m_Offset = 0;

        public FeedPage()
        {
            InitializeComponent();
            OnPostScrollEnd = new BasicCommand(o => true, async ctx =>
              {
                  PostRequest request = new PostRequest()
                  {
                      count = m_PostsCount,
                      offset = m_Offset,
                      type = (PostRequest.EPostType)lstPostType.SelectedItem
                  };
                  await LoadNewPosts(request);
              });
            DataContext = this;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        async Task<bool> UpdatePosts(PostRequest filters)
        {
            postsLists.Children.Clear();
            m_Offset = 0;
            var postsResponse = await PostAPI.GetAllPosts(filters, AppPersistent.UserToken);

            if (postsResponse == null) { return false; }
            AddPostsToList(postsResponse.data);
            return true;
        }

        async Task<bool> LoadNewPosts(PostRequest filter)
        {
            var postsResponse = await PostAPI.GetAllPosts(filter, AppPersistent.UserToken);

            if (postsResponse == null) { return false; }
            AddPostsToList(postsResponse.data);
            return true;

        }

        void AddPostsToList(List<PostData> posts)
        {
            foreach (var post in posts)
            {
                PostWidget widget = new PostWidget()
                {
                    PostText = post.text,
                    ImageURL = post.attachments[0].photo.photo_medium,
                    CurrentPostData = post
                };
                widget.PostClick += OnPost_Click;
                postsLists.Children.Add(widget);
            }
            m_Offset += m_PostsCount;
        }


        private async void lstPostType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadingRing.Visibility = Visibility.Visible;
            postsLists.Children.Clear();

            PostRequest request = new PostRequest()
            {
                count = m_PostsCount,
                type = (PostRequest.EPostType)lstPostType.SelectedItem
            };
            var postsResponse = await PostAPI.GetAllPosts(request, AppPersistent.UserToken);
            AddPostsToList(postsResponse.data);
            loadingRing.Visibility = Visibility.Hidden;

        }

        private void OnPost_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.OpenOverlay(new PostOverlayPage() { PostInfo = (sender as PostWidget).CurrentPostData });
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubpage(new SubmitPostPage());
        }
    }
}

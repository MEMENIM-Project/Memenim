
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

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for PostsPage.xaml
    /// </summary>
    public partial class PostsPage : UserControl
    {
        public ICommand OnPostScrollEnd { get; set; }

        public List<PostRequest.EPostType> PostTypes { get; } = new List<PostRequest.EPostType>()
        { PostRequest.EPostType.Popular, PostRequest.EPostType.New, PostRequest.EPostType.My, PostRequest.EPostType.Favorite };

        private int m_PostsCount = 20;
        private int m_Offset = 0;


        public PostsPage()
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
            loadingRing.Visibility = Visibility.Visible;

            PostRequest request = new PostRequest()
            {
                count = m_PostsCount,
                type = (PostRequest.EPostType)lstPostType.SelectedItem
            };
            await UpdatePosts(request);
            loadingRing.Visibility = Visibility.Hidden;
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
                postsLists.Children.Add(widget);
            }
            m_Offset += m_PostsCount;
        }


        private void lstPostType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadingRing.Visibility = Visibility.Visible;
            m_Offset = 0;
            postsLists.Children.Clear();
            loadingRing.Visibility = Visibility.Hidden;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubpage(new SubmitPostPage());
        }
    }
}

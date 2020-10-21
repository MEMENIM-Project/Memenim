using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Utils;
using Memenim.Widgets;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Managers;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for PostsPage.xaml
    /// </summary>
    public partial class FeedPage : PageContent
    {
        public ICommand OnPostScrollEnd { get; set; }

        public List<PostType> PostTypes { get; } = new List<PostType>()
        { PostType.Popular, PostType.New, PostType.My, PostType.Favorite };

        private int m_PostsCount = 20;
        private int m_Offset = 0;

        public FeedPage() : base()
        {
            InitializeComponent();
            OnPostScrollEnd = new BasicCommand(o => true, async ctx =>
              {
                  PostRequest request = new PostRequest()
                  {
                      count = m_PostsCount,
                      offset = m_Offset,
                      type = (PostType)lstPostType.SelectedItem
                  };
                  await LoadNewPosts(request);
              });
            DataContext = this;
        }

        async Task<bool> UpdatePosts(PostRequest filters)
        {
            postsLists.Children.Clear();
            m_Offset = 0;
            var postsResponse = await PostApi.GetAll(filters, AppPersistent.UserToken);

            if (postsResponse == null) { return false; }
            AddPostsToList(postsResponse.data);
            return true;
        }

        async Task<bool> LoadNewPosts(PostRequest filter)
        {
            var postsResponse = await PostApi.GetAll(filter, AppPersistent.UserToken);

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
                type = (PostType)lstPostType.SelectedItem
            };
            var postsResponse = await PostApi.GetAll(request, AppPersistent.UserToken);
            AddPostsToList(postsResponse.data);
            loadingRing.Visibility = Visibility.Hidden;

        }

        private void OnPost_Click(object sender, RoutedEventArgs e)
        {
            OverlayPageController.Instance.RequestOverlay<PostOverlayPage>();
            //PageNavigationManager.OpenOverlay(new PostOverlayPage() { PostInfo = (sender as PostWidget).CurrentPostData });
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubpage<SubmitPostPage>();
        }
    }
}

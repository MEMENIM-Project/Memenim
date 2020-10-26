using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Commands;
using Memenim.Widgets;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class FeedPage : UserControl
    {
        private const int OffsetPerTime = 20;

        private int _offset;

        public List<PostType> PostTypes { get; } = new List<PostType>
        {
            PostType.Popular,
            PostType.New,
            PostType.My,
            PostType.Favorite
        };
        public ICommand OnPostScrollEnd { get; set; }

        public FeedPage()
        {
            InitializeComponent();
            DataContext = this;
            OnPostScrollEnd = new BasicCommand(_ => true, async _ =>
            {
                PostRequest request = new PostRequest
                {
                    count = OffsetPerTime,
                    offset = _offset,
                    type = (PostType)lstPostType.SelectedItem
                };

                await LoadNewPosts(request)
                    .ConfigureAwait(true);
            });
        }

        private Task UpdatePosts()
        {
            PostRequest request = new PostRequest()
            {
                count = OffsetPerTime,
                offset = 0,
                type = (PostType)lstPostType.SelectedItem
            };

            return UpdatePosts(request);
        }
        private async Task UpdatePosts(PostRequest request)
        {
            loadingRing.Visibility = Visibility.Visible;

            postsLists.Children.Clear();
            _offset = 0;

            var postsResponse = await PostApi.GetAll(request, SettingManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (postsResponse == null)
                return;

            AddPostsToList(postsResponse.data);

            loadingRing.Visibility = Visibility.Hidden;
        }

        private async Task LoadNewPosts(PostRequest request)
        {
            var postsResponse = await PostApi.GetAll(request, SettingManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (postsResponse == null)
                return;

            AddPostsToList(postsResponse.data);
        }

        private void AddPostsToList(List<PostData> posts)
        {
            foreach (var post in posts)
            {
                PostWidget widget = new PostWidget()
                {
                    CurrentPostData = post
                };
                widget.PostClick += OnPost_Click;

                postsLists.Children.Add(widget);
            }

            _offset += OffsetPerTime;
        }

        private async void lstPostType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private void OnPost_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.OpenOverlay(new PostOverlayPage
            {
                CurrentPostData = (sender as PostWidget)?.CurrentPostData
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            PageNavigationManager.SwitchToSubPage(new SubmitPostPage());
        }
    }
}

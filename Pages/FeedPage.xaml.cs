using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Commands;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Navigation;
using Memenim.Settings;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class FeedPage : PageContent
    {
        public static readonly DependencyProperty OnPostScrollEndProperty =
            DependencyProperty.Register("OnPostScrollEnd", typeof(ICommand), typeof(FeedPage),
                new PropertyMetadata(new BasicCommand(_ => false)));

        private const int OffsetPerTime = 20;

        private int _offset;

        public ICommand OnPostScrollEnd
        {
            get
            {
                return (ICommand)GetValue(OnPostScrollEndProperty);
            }
            set
            {
                SetValue(OnPostScrollEndProperty, value);
            }
        }

        public FeedPage()
        {
            InitializeComponent();
            DataContext = this;

            OnPostScrollEnd = new BasicCommand(
                _ => true, async _ => await LoadNewPosts()
                    .ConfigureAwait(true)
                );

            lstPostTypes.SelectedIndex = 0;
        }

        private Task UpdatePosts()
        {
            return UpdatePosts(0);
        }
        private Task UpdatePosts(int offset)
        {
            PostRequest request = new PostRequest
            {
                count = OffsetPerTime,
                offset = offset,
                type = ((PostTypeNode)lstPostTypes.SelectedItem).CategoryType
            };

            return UpdatePosts(request);
        }
        private async Task UpdatePosts(PostRequest request)
        {
            loadingRing.Visibility = Visibility.Visible;

            postsList.Children.Clear();

            _offset = 0;

            var postsResponse = await PostApi.GetAll(request, SettingsManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (postsResponse == null)
                return;

            AddPostsToList(postsResponse.data);

            loadingRing.Visibility = Visibility.Hidden;
        }

        private Task LoadNewPosts()
        {
            return LoadNewPosts(_offset);
        }
        private Task LoadNewPosts(int offset)
        {
            PostRequest request = new PostRequest
            {
                count = OffsetPerTime,
                offset = offset,
                type = ((PostTypeNode)lstPostTypes.SelectedItem).CategoryType
            };

            return LoadNewPosts(request);
        }
        private async Task LoadNewPosts(PostRequest request)
        {
            loadingRing.Visibility = Visibility.Visible;

            var postsResponse = await PostApi.GetAll(request, SettingsManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (postsResponse == null)
                return;

            AddPostsToList(postsResponse.data);

            loadingRing.Visibility = Visibility.Hidden;
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

                postsList.Children.Add(widget);
            }

            _offset += OffsetPerTime;
        }

        private async void lstPostTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private void OnPost_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayPage
            {
                CurrentPostData = (sender as PostWidget)?.CurrentPostData
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<SubmitPostPage>();
        }
    }
}

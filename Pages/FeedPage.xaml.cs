using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Commands;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Settings;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class FeedPage : PageContent
    {
        public static readonly DependencyProperty OnPostScrollEndProperty =
            DependencyProperty.Register(nameof(OnPostScrollEnd), typeof(ICommand), typeof(FeedPage),
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
                _ => true, async _ =>
                {
                    if (svPosts.HorizontalOffset == 0)
                        return;

                    await LoadNewPosts()
                        .ConfigureAwait(true);
                });

            lstPostTypes.SelectedIndex = 0;
        }

        private Task UpdatePosts()
        {
            return UpdatePosts(((PostTypeNode)lstPostTypes.SelectedItem).CategoryType,
                OffsetPerTime);
        }
        private async Task UpdatePosts(PostType type, int count)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            lstPosts.Children.Clear();
            svPosts.ScrollToHorizontalOffset(0);

            _offset = 0;

            await LoadNewPosts(type, count, _offset)
                .ConfigureAwait(true);
        }

        private Task LoadNewPosts()
        {
            return LoadNewPosts(_offset);
        }
        private Task LoadNewPosts(int offset)
        {
            return LoadNewPosts(((PostTypeNode)lstPostTypes.SelectedItem).CategoryType,
                OffsetPerTime, offset);
        }
        private async Task LoadNewPosts(PostType type, int count, int offset)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            svPosts.IsEnabled = false;

            var postsResponse = await PostApi.Get(SettingsManager.PersistentSettings.CurrentUserToken,
                    type, count, offset)
                .ConfigureAwait(true);

            if (postsResponse == null)
                return;

            await AddPostsToList(postsResponse.data)
                .ConfigureAwait(true);

            svPosts.IsEnabled = true;

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);
        }

        private Task AddPostsToList(List<PostSchema> posts)
        {
            return Task.Run(() =>
            {
                foreach (var post in posts)
                {
                    Dispatcher.Invoke(() =>
                    {
                        PostWidget widget = new PostWidget
                        {
                            CurrentPostData = post
                        };
                        widget.PostClick += OnPost_Click;

                        lstPosts.Children.Add(widget);
                    });
                }

                _offset += OffsetPerTime;
            });
        }

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (opacity < 0.7)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        loadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    loadingGrid.Visibility = Visibility.Collapsed;
                });
            });
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Commands;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class FeedPage : PageContent
    {
        private const int OffsetPerTime = 20;

        public FeedViewModel ViewModel
        {
            get
            {
                return DataContext as FeedViewModel;
            }
        }

        public FeedPage()
        {
            InitializeComponent();
            DataContext = new FeedViewModel();

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

            ViewModel.Offset = 0;

            await LoadMorePosts(type, count, ViewModel.Offset)
                .ConfigureAwait(true);
        }

        private Task LoadMorePosts()
        {
            return LoadMorePosts(ViewModel.Offset);
        }
        private Task LoadMorePosts(int offset)
        {
            return LoadMorePosts(((PostTypeNode)lstPostTypes.SelectedItem).CategoryType,
                OffsetPerTime, offset);
        }
        private async Task LoadMorePosts(PostType type, int count, int offset)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            svPosts.IsEnabled = false;

            var result = await PostApi.Get(SettingsManager.PersistentSettings.CurrentUserToken,
                    type, count, offset)
                .ConfigureAwait(true);

            if (result == null)
                return;

            await AddMorePosts(result.data)
                .ConfigureAwait(true);

            svPosts.IsEnabled = true;

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);
        }

        private Task AddMorePosts(List<PostSchema> posts)
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

                Dispatcher.Invoke(() =>
                {
                    ViewModel.Offset += posts.Count;
                });
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

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            ViewModel.OnPostScrollEnd = new BasicCommand(
                _ => true, async _ =>
                {
                    if (svPosts.HorizontalOffset == 0)
                        return;

                    await LoadMorePosts()
                        .ConfigureAwait(true);
                });
        }

        private async void lstPostTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private void OnPost_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayViewModel
            {
                CurrentPostData = (sender as PostWidget)?.CurrentPostData
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<SubmitPostPage>();
        }

        private void SvPosts_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.HorizontalOffset;
        }
    }
}

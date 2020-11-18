using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Pages;
using Memenim.Settings;

namespace Memenim.Widgets
{
    public partial class CommentsList : UserControl
    {
        public static readonly RoutedEvent OnCommentDeleted =
            EventManager.RegisterRoutedEvent(nameof(CommentDelete), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(CommentsList),
                new PropertyMetadata(-1, PostIdChangedCallback));
        public static readonly DependencyProperty CommentsCountProperty =
            DependencyProperty.Register(nameof(CommentsCount), typeof(StatisticSchema), typeof(CommentsList),
                new PropertyMetadata(new StatisticSchema()));

        public event EventHandler<RoutedEventArgs> CommentDelete
        {
            add
            {
                AddHandler(OnCommentDeleted, value);
            }
            remove
            {
                RemoveHandler(OnCommentDeleted, value);
            }
        }

        private const int OffsetPerTime = 20;

        private readonly Timer _autoUpdateTimer;
        private bool _postIdChangedUpdate;

        public int PostId
        {
            get
            {
                return (int)GetValue(PostIdProperty);
            }
            set
            {
                SetValue(PostIdProperty, value);
            }
        }
        public StatisticSchema CommentsCount
        {
            get
            {
                return (StatisticSchema)GetValue(CommentsCountProperty);
            }
            set
            {
                SetValue(CommentsCountProperty, value);
            }
        }
        public int Offset { get; set; }

        public CommentsList()
        {
            InitializeComponent();
            DataContext = this;

            _autoUpdateTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
            _autoUpdateTimer.Elapsed += AutoUpdateTimerCallback;
            _autoUpdateTimer.Stop();
        }

        public Task UpdateComments(bool resetScroll = true)
        {
            lstComments.Children.Clear();

            Offset = 0;
            btnLoadMore.Visibility = Visibility.Visible;

            if (resetScroll)
            {
                PostOverlayPage page = this.TryFindParent<PostOverlayPage>();

                page?.svPost?.ScrollToVerticalOffset(0.0);
            }

            return LoadMoreComments();
        }

        public async Task LoadMoreComments()
        {
            if (lstComments.Children.Count != 0)
            {
                _autoUpdateTimer.Stop();

                Offset += await GetNewCommentsCount()
                    .ConfigureAwait(true);

                _autoUpdateTimer.Start();
            }

            await LoadMoreComments(Offset)
                .ConfigureAwait(true);
        }
        public Task LoadMoreComments(int offset)
        {
            return LoadMoreComments(OffsetPerTime, offset);
        }
        public async Task LoadMoreComments(int count, int offset)
        {
            var result = await PostApi.GetComments(SettingsManager.PersistentSettings.CurrentUserToken,
                    PostId, count, offset)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", "Cannot load comments")
                    .ConfigureAwait(true);
                return;
            }

            await AddMoreComments(result.data)
                .ConfigureAwait(true);
        }

        public Task AddMoreComments(List<CommentSchema> comments)
        {
            return Task.Run(() =>
            {
                foreach (var comment in comments)
                {
                    Dispatcher.Invoke(() =>
                    {
                        UserComment commentWidget = new UserComment
                        {
                            CurrentCommentData = comment
                        };
                        commentWidget.CommentDelete += Comment_CommentDelete;

                        lstComments.Children.Insert(0, commentWidget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    if (lstComments.Children.Count >= CommentsCount.count)
                        btnLoadMore.Visibility = Visibility.Collapsed;

                    Offset += comments.Count;
                });
            });
        }

        public Task<int> GetNewCommentsCount(int offset = 0)
        {
            return GetNewCommentsCount(OffsetPerTime, offset);
        }
        public async Task<int> GetNewCommentsCount(int countPerTime, int offset)
        {
            int commentsCount = 0;

            Dispatcher.Invoke(() =>
            {
                commentsCount = lstComments.Children.Count;
            });

            if (commentsCount == 0)
            {
                await Dispatcher.Invoke(async () =>
                {
                    await UpdateComments(false)
                        .ConfigureAwait(true);
                }).ConfigureAwait(true);

                return 0;
            }

            int headOldId = -1;

            Dispatcher.Invoke(() =>
            {
                headOldId = (lstComments.Children[^1] as UserComment)?
                    .CurrentCommentData.id ?? -1;
            });

            if (headOldId == -1)
                return 0;

            int postId = -1;

            Dispatcher.Invoke(() =>
            {
                postId = PostId;
            });

            if (postId == -1)
                return 0;

            return await Task.Run(async () =>
            {
                int countNew = 0;
                bool headOldIsFound = false;

                while (!headOldIsFound)
                {
                    var result = await PostApi.GetComments(SettingsManager.PersistentSettings.CurrentUserToken,
                            postId, countPerTime, offset)
                        .ConfigureAwait(false);

                    if (result?.error != false)
                        continue;

                    if (result.data.Count == 0)
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            await UpdateComments(false)
                                .ConfigureAwait(true);
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    foreach (var comment in result.data)
                    {
                        if (comment.id == headOldId)
                        {
                            headOldIsFound = true;
                            break;
                        }

                        ++countNew;
                    }

                    if (countNew >= 500)
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            await UpdateComments(false)
                                .ConfigureAwait(true);
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    offset += countPerTime;
                }

                return countNew;
            }).ConfigureAwait(true);
        }

        public async Task LoadNewComments(int offset = 0)
        {
            _autoUpdateTimer.Stop();

            int count = await GetNewCommentsCount(offset)
                .ConfigureAwait(true);

            if (count == 0)
            {
                _autoUpdateTimer.Start();
                return;
            }

            int postId = -1;

            Dispatcher.Invoke(() =>
            {
                postId = PostId;
            });

            if (postId == -1)
                return;

            var result = await PostApi.GetComments(SettingsManager.PersistentSettings.CurrentUserToken,
                    postId, count, offset)
                .ConfigureAwait(true);

            if (result.error)
            {
                await Dispatcher.Invoke(async () =>
                {
                    await DialogManager.ShowDialog("F U C K", "Cannot load comments")
                        .ConfigureAwait(true);
                }).ConfigureAwait(true);

                _autoUpdateTimer.Start();
                return;
            }

            PostOverlayPage page = null;
            double verticalOffset = 0.0;
            double extentHeight = 1.0;

            Dispatcher.Invoke(() =>
            {
                page = this.TryFindParent<PostOverlayPage>();
                verticalOffset = page?.svPost?.VerticalOffset ?? 0.0;
                extentHeight = page?.svPost?.ScrollableHeight ?? 1.0;
            });

            await AddNewComments(result.data)
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                if (verticalOffset >= extentHeight)
                    page?.svPost?.ScrollToEnd();
            });

            _autoUpdateTimer.Start();
        }

        public Task AddNewComments(List<CommentSchema> comments)
        {
            return Task.Run(() =>
            {
                for (var i = comments.Count - 1; i >= 0; --i)
                {
                    var comment = comments[i];

                    Dispatcher.Invoke(() =>
                    {
                        UserComment commentWidget = new UserComment
                        {
                            CurrentCommentData = comment
                        };
                        commentWidget.CommentDelete += Comment_CommentDelete;

                        lstComments.Children.Add(commentWidget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    Offset += comments.Count;
                });
            });
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_postIdChangedUpdate)
            {
                _autoUpdateTimer.Start();
                return;
            }

            await UpdateComments()
                .ConfigureAwait(true);

            _autoUpdateTimer.Start();
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoUpdateTimer.Stop();
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        private static async void PostIdChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommentsList commentsList = d as CommentsList;

            if (commentsList == null)
                return;

            commentsList._postIdChangedUpdate = true;

            await commentsList.UpdateComments()
                .ConfigureAwait(true);

            commentsList._postIdChangedUpdate = false;
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        private async void AutoUpdateTimerCallback(object sender, ElapsedEventArgs e)
        {
            await LoadNewComments()
                .ConfigureAwait(true);
        }

        private async void btnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            btnLoadMore.IsEnabled = false;

            await LoadMoreComments()
                .ConfigureAwait(true);

            btnLoadMore.IsEnabled = true;
        }

        private void Comment_CommentDelete(object sender, RoutedEventArgs e)
        {
            UserComment comment = sender as UserComment;

            if (comment == null)
                return;

            lstComments.Children.Remove(comment);

            --Offset;

            RaiseEvent(new RoutedEventArgs(OnCommentDeleted));
        }
    }
}

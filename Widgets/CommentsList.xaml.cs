using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Pages;
using Memenim.Settings;
using Memenim.Utils;
using WpfAnimatedGif;

namespace Memenim.Widgets
{
    public partial class CommentsList : WidgetContent
    {
        public static readonly RoutedEvent OnCommentsUpdated =
            EventManager.RegisterRoutedEvent(nameof(CommentsUpdate), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        public static readonly RoutedEvent OnCommentReplied =
            EventManager.RegisterRoutedEvent(nameof(CommentReply), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        public static readonly RoutedEvent OnCommentDeleted =
            EventManager.RegisterRoutedEvent(nameof(CommentDelete), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(CommentsList),
                new PropertyMetadata(-1, PostIdChangedCallback));
        public static readonly DependencyProperty CommentsCountProperty =
            DependencyProperty.Register(nameof(CommentsCount), typeof(StatisticSchema), typeof(CommentsList),
                new PropertyMetadata(new StatisticSchema(), CommentsCountChangedCallback));

        public event EventHandler<RoutedEventArgs> CommentsUpdate
        {
            add
            {
                AddHandler(OnCommentsUpdated, value);
            }
            remove
            {
                RemoveHandler(OnCommentsUpdated, value);
            }
        }
        public event EventHandler<RoutedEventArgs> CommentReply
        {
            add
            {
                AddHandler(OnCommentReplied, value);
            }
            remove
            {
                RemoveHandler(OnCommentReplied, value);
            }
        }
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

        public PageStateType PageState { get; set; }
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
            foreach (var comment in lstComments.Children)
            {
                UserComment userComment = comment as UserComment;

                if (userComment == null)
                    continue;

                ImageBehavior.SetAnimatedSource(userComment.AvatarImage, null);
            }

            lstComments.Children.Clear();

            Offset = 0;
            btnLoadMore.Visibility = Visibility.Collapsed;

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

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
            var result = await PostApi.GetComments(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    PostId, count, offset)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                var message = LocalizationUtils.GetLocalized("CouldNotLoadCommentsMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);
                return;
            }

            await AddMoreComments(result.Data)
                .ConfigureAwait(true);

            RaiseEvent(new RoutedEventArgs(OnCommentsUpdated));
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
                        commentWidget.CommentReply += Comment_CommentReply;
                        commentWidget.CommentDelete += Comment_CommentDelete;

                        lstComments.Children.Insert(0, commentWidget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    btnLoadMore.Visibility =
                        lstComments.Children.Count >= CommentsCount.TotalCount
                            ? Visibility.Collapsed
                            : Visibility.Visible;

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
                await Dispatcher.Invoke(() =>
                {
                    return UpdateComments(false);
                }).ConfigureAwait(true);

                return 0;
            }

            int headOldId = -1;

            Dispatcher.Invoke(() =>
            {
                headOldId = (lstComments.Children[^1] as UserComment)?
                    .CurrentCommentData.Id ?? -1;
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
                    var result = await PostApi.GetComments(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            postId, countPerTime, offset)
                        .ConfigureAwait(false);

                    if (result?.IsError != false)
                        continue;

                    if (result.Data.Count == 0)
                    {
                        await Dispatcher.Invoke(() =>
                        {
                            return UpdateComments(false);
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    foreach (var comment in result.Data)
                    {
                        if (comment.Id == headOldId)
                        {
                            headOldIsFound = true;
                            break;
                        }

                        ++countNew;
                    }

                    if (countNew >= 500)
                    {
                        await Dispatcher.Invoke(() =>
                        {
                            return UpdateComments(false);
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

            var result = await PostApi.GetComments(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    postId, count, offset)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await Dispatcher.Invoke(() =>
                {
                    var message = LocalizationUtils.GetLocalized("CouldNotLoadCommentsMessage");

                    return DialogManager.ShowErrorDialog(message);
                }).ConfigureAwait(true);

                _autoUpdateTimer.Start();
                return;
            }

            PostOverlayPage page = null;
            double verticalOffset = 0.0;
            double scrollableHeight = 21.0;

            Dispatcher.Invoke(() =>
            {
                page = this.TryFindParent<PostOverlayPage>();
                verticalOffset = page?.svPost?.VerticalOffset ?? verticalOffset;
                scrollableHeight = page?.svPost?.ScrollableHeight ?? scrollableHeight;
            });

            await AddNewComments(result.Data)
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                if (verticalOffset >= scrollableHeight - 20)
                    page?.svPost?.ScrollToEnd();
            });

            RaiseEvent(new RoutedEventArgs(OnCommentsUpdated));

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
                        commentWidget.CommentReply += Comment_CommentReply;
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
            PageState = PageStateType.Loaded;

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

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
            PageState = PageStateType.Unloaded;

            _autoUpdateTimer.Stop();

            foreach (var comment in lstComments.Children)
            {
                UserComment userComment = comment as UserComment;

                if (userComment == null)
                    continue;

                ImageBehavior.SetAnimatedSource(userComment.AvatarImage, null);
            }

            lstComments.Children.Clear();

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        private static async void PostIdChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
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

        private static void CommentsCountChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            CommentsList commentsList = d as CommentsList;

            if (commentsList == null)
                return;

            commentsList.btnLoadMore.Visibility =
                commentsList.lstComments.Children.Count >= ((StatisticSchema)e.NewValue).TotalCount
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }

        private async void AutoUpdateTimerCallback(object sender, ElapsedEventArgs e)
        {
            if (!_autoUpdateTimer.Enabled)
                return;

            if (PageState != PageStateType.Loaded)
                return;

            await LoadNewComments()
                .ConfigureAwait(true);
        }

        private async void btnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            btnLoadMore.IsEnabled = false;

            PostOverlayPage page = null;
            double scrollableHeight = 0.0;

            Dispatcher.Invoke(() =>
            {
                page = this.TryFindParent<PostOverlayPage>();
                scrollableHeight = page?.svPost?.ScrollableHeight ?? scrollableHeight;
            });

            await LoadMoreComments()
                .ConfigureAwait(true);

            UpdateLayout();

            if (page?.svPost?.VerticalOffset > page?.wdgPost?.ActualHeight - (page?.svPost?.ActualHeight / 100 * 50))
            {
                page?.svPost?.ScrollToVerticalOffset(
                    page.svPost.VerticalOffset + (page.svPost.ScrollableHeight - scrollableHeight));
            }

            btnLoadMore.IsEnabled = true;
        }

        private void Comment_CommentReply(object sender, RoutedEventArgs e)
        {
            UserComment comment = sender as UserComment;

            if (comment == null)
                return;

            RaiseEvent(new RoutedEventArgs(OnCommentReplied, sender));
        }

        private void Comment_CommentDelete(object sender, RoutedEventArgs e)
        {
            _autoUpdateTimer.Stop();

            UserComment comment = sender as UserComment;

            if (comment == null)
            {
                _autoUpdateTimer.Start();
                return;
            }

            lstComments.Children.Remove(comment);

            --Offset;

            RaiseEvent(new RoutedEventArgs(OnCommentDeleted));

            _autoUpdateTimer.Start();
        }
    }
}

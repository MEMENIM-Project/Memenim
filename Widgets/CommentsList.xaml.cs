using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Generic;
using Memenim.Pages;
using Memenim.Settings;
using Memenim.Utils;
using WpfAnimatedGif;

namespace Memenim.Widgets
{
    public partial class CommentsList : WidgetContent
    {
        public static readonly RoutedEvent CommentsUpdatedEvent =
            EventManager.RegisterRoutedEvent(nameof(CommentsUpdated), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        public static readonly RoutedEvent CommentReplyEvent =
            EventManager.RegisterRoutedEvent(nameof(CommentReply), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        public static readonly RoutedEvent CommentDeleteEvent =
            EventManager.RegisterRoutedEvent(nameof(CommentDelete), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(CommentsList));
        

        
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(CommentsList),
                new PropertyMetadata(-1, PostIdChangedCallback));
        public static readonly DependencyProperty CommentsCountProperty =
            DependencyProperty.Register(nameof(CommentsCount), typeof(StatisticSchema), typeof(CommentsList),
                new PropertyMetadata(new StatisticSchema(), CommentsCountChangedCallback));



        public event EventHandler<RoutedEventArgs> CommentsUpdated
        {
            add
            {
                AddHandler(CommentsUpdatedEvent, value);
            }
            remove
            {
                RemoveHandler(CommentsUpdatedEvent, value);
            }
        }
        public event EventHandler<RoutedEventArgs> CommentReply
        {
            add
            {
                AddHandler(CommentReplyEvent, value);
            }
            remove
            {
                RemoveHandler(CommentReplyEvent, value);
            }
        }
        public event EventHandler<RoutedEventArgs> CommentDelete
        {
            add
            {
                AddHandler(CommentDeleteEvent, value);
            }
            remove
            {
                RemoveHandler(CommentDeleteEvent, value);
            }
        }



        private const int OffsetPerTime = 20;



        private readonly Timer _autoUpdateCommentsTimer;

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

            _autoUpdateCommentsTimer = new Timer
            {
                Interval = TimeSpan
                    .FromSeconds(15)
                    .TotalMilliseconds
            };
            _autoUpdateCommentsTimer.Elapsed += AutoUpdateCommentsTimer_Tick;
        }



        private static void PostIdChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as CommentsList;

            target?.OnPostIdChanged(e);
        }

        private static void CommentsCountChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as CommentsList;

            target?.OnCommentsCountChanged(e);
        }



#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        private async void OnPostIdChanged(
            DependencyPropertyChangedEventArgs e)
        {
            _postIdChangedUpdate = true;

            await UpdateComments()
                .ConfigureAwait(true);

            _postIdChangedUpdate = false;
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        private void OnCommentsCountChanged(
            DependencyPropertyChangedEventArgs e)
        {
            LoadMoreButton.Visibility =
                CommentsWrapPanel.Children.Count >= ((StatisticSchema)e.NewValue).TotalCount
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }



        public Task UpdateComments(
            bool resetScroll = true)
        {
            foreach (var element in CommentsWrapPanel.Children)
            {
                if (!(element is Comment comment))
                    continue;

                ImageBehavior.SetAnimatedSource(
                    comment.Info.Avatar.Image, null);
            }

            CommentsWrapPanel.Children.Clear();

            Offset = 0;
            LoadMoreButton.Visibility = Visibility.Collapsed;

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (resetScroll)
            {
                var page = this.TryFindParent<PostOverlayPage>();

                page?.svPost.ScrollToVerticalOffset(0.0);
            }

            return LoadMoreComments();
        }


        public async Task LoadMoreComments()
        {
            if (CommentsWrapPanel.Children.Count != 0)
            {
                _autoUpdateCommentsTimer.Stop();

                Offset += await GetNewCommentsCount()
                    .ConfigureAwait(true);

                _autoUpdateCommentsTimer.Start();
            }

            await LoadMoreComments(Offset)
                .ConfigureAwait(true);
        }
        public Task LoadMoreComments(
            int offset)
        {
            return LoadMoreComments(
                OffsetPerTime, offset);
        }
        public async Task LoadMoreComments(
            int count, int offset)
        {
            var result = await PostApi.GetComments(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    PostId, count, offset)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                var message = LocalizationUtils
                    .GetLocalized("FailedToLoadCommentsMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            await AddMoreComments(result.Data)
                .ConfigureAwait(true);

            RaiseEvent(new RoutedEventArgs(CommentsUpdatedEvent));
        }

        public Task AddMoreComments(
            List<CommentSchema> comments)
        {
            return Task.Run(() =>
            {
                foreach (var comment in comments)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var commentWidget = new Comment
                        {
                            CurrentCommentData = comment
                        };
                        commentWidget.CommentReply += Comment_Reply;
                        commentWidget.CommentDelete += Comment_Delete;

                        CommentsWrapPanel.Children.Insert(0, commentWidget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    LoadMoreButton.Visibility =
                        CommentsWrapPanel.Children.Count >= CommentsCount.TotalCount
                            ? Visibility.Collapsed
                            : Visibility.Visible;

                    Offset += comments.Count;
                });
            });
        }



        public Task<int> GetNewCommentsCount(
            int offset = 0)
        {
            return GetNewCommentsCount(
                OffsetPerTime, offset);
        }
        public async Task<int> GetNewCommentsCount(
            int countPerTime, int offset)
        {
            var commentsCount = 0;

            Dispatcher.Invoke(() =>
            {
                commentsCount = CommentsWrapPanel.Children.Count;
            });

            if (commentsCount == 0)
            {
                await Dispatcher.Invoke(() =>
                {
                    return UpdateComments(false);
                }).ConfigureAwait(true);

                return 0;
            }

            var headOldId = -1;

            Dispatcher.Invoke(() =>
            {
                headOldId = (CommentsWrapPanel.Children[^1] as Comment)?
                    .CurrentCommentData.Id ?? -1;
            });

            if (headOldId == -1)
                return 0;

            var postId = -1;

            Dispatcher.Invoke(() =>
            {
                postId = PostId;
            });

            if (postId == -1)
                return 0;

            return await Task.Run(async () =>
            {
                var countNew = 0;
                var headOldIsFound = false;

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


        public async Task LoadNewComments(
            int offset = 0)
        {
            _autoUpdateCommentsTimer.Stop();

            var count = await GetNewCommentsCount(offset)
                .ConfigureAwait(true);

            if (count == 0)
            {
                _autoUpdateCommentsTimer.Start();
                return;
            }

            var postId = -1;

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
                    var message = LocalizationUtils
                        .GetLocalized("FailedToLoadCommentsMessage");

                    return DialogManager.ShowErrorDialog(message);
                }).ConfigureAwait(true);

                _autoUpdateCommentsTimer.Start();

                return;
            }

            PostOverlayPage page = null;
            var verticalOffset = 0.0;
            var scrollableHeight = 21.0;

            Dispatcher.Invoke(() =>
            {
                page = this.TryFindParent<PostOverlayPage>();
                verticalOffset = page?.svPost.VerticalOffset ?? verticalOffset;
                scrollableHeight = page?.svPost.ScrollableHeight ?? scrollableHeight;
            });

            await AddNewComments(result.Data)
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                if (verticalOffset >= scrollableHeight - 20)
                    page?.svPost.ScrollToEnd();
            });

            RaiseEvent(new RoutedEventArgs(CommentsUpdatedEvent));

            _autoUpdateCommentsTimer.Start();
        }

        public Task AddNewComments(
            List<CommentSchema> comments)
        {
            return Task.Run(() =>
            {
                for (var i = comments.Count - 1; i >= 0; --i)
                {
                    var comment = comments[i];

                    Dispatcher.Invoke(() =>
                    {
                        var commentWidget = new Comment
                        {
                            CurrentCommentData = comment
                        };
                        commentWidget.CommentReply += Comment_Reply;
                        commentWidget.CommentDelete += Comment_Delete;

                        CommentsWrapPanel.Children.Add(commentWidget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    Offset += comments.Count;
                });
            });
        }



        protected override async void OnEnter(object sender,
            RoutedEventArgs e)
        {
            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            if (_postIdChangedUpdate)
            {
                _autoUpdateCommentsTimer.Start();

                return;
            }

            await UpdateComments()
                .ConfigureAwait(true);

            _autoUpdateCommentsTimer.Start();
        }

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            _autoUpdateCommentsTimer.Stop();

            foreach (var element in CommentsWrapPanel.Children)
            {
                if (!(element is Comment comment))
                    continue;

                ImageBehavior.SetAnimatedSource(
                    comment.Info.Avatar.Image, null);
            }

            CommentsWrapPanel.Children.Clear();

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            base.OnExit(sender, e);

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private void Comment_Reply(object sender,
            RoutedEventArgs e)
        {
            if (!(sender is Comment))
                return;

            RaiseEvent(new RoutedEventArgs(CommentReplyEvent, sender));
        }

        private void Comment_Delete(object sender,
            RoutedEventArgs e)
        {
            _autoUpdateCommentsTimer.Stop();

            if (!(sender is Comment comment))
            {
                _autoUpdateCommentsTimer.Start();

                return;
            }

            CommentsWrapPanel.Children.Remove(
                comment);

            --Offset;

            RaiseEvent(new RoutedEventArgs(CommentDeleteEvent, sender));

            _autoUpdateCommentsTimer.Start();
        }

        private async void LoadMoreButton_Click(object sender,
            RoutedEventArgs e)
        {
            LoadMoreButton.IsEnabled = false;

            PostOverlayPage page = null;
            var scrollableHeight = 0.0;

            Dispatcher.Invoke(() =>
            {
                page = this.TryFindParent<PostOverlayPage>();
                scrollableHeight = page?.svPost.ScrollableHeight ?? scrollableHeight;
            });

            await LoadMoreComments()
                .ConfigureAwait(true);

            UpdateLayout();

            if (page?.svPost != null
                && page.svPost.VerticalOffset > page.wdgPost.ActualHeight - (page.svPost.ActualHeight / 100 * 50))
            {
                page.svPost.ScrollToVerticalOffset(
                    page.svPost.VerticalOffset + (page.svPost.ScrollableHeight - scrollableHeight));
            }

            LoadMoreButton.IsEnabled = true;
        }



        private async void AutoUpdateCommentsTimer_Tick(object sender,
            ElapsedEventArgs e)
        {
            if (!_autoUpdateCommentsTimer.Enabled)
                return;

            if (State != ControlStateType.Loaded)
                return;

            await LoadNewComments()
                .ConfigureAwait(true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Pages;

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

        private int _offset;
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

        public CommentsList()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Task UpdateComments()
        {
            lstComments.Children.Clear();

            _offset = 0;
            btnLoadMore.Visibility = Visibility.Visible;

            PostOverlayPage page = this.TryFindParent<PostOverlayPage>();

            page?.svPost.ScrollToVerticalOffset(0.0);

            return LoadMoreComments();
        }

        public Task LoadMoreComments()
        {
            return LoadMoreComments(_offset);
        }
        public Task LoadMoreComments(int offset)
        {
            return LoadMoreComments(OffsetPerTime, offset);
        }
        public async Task LoadMoreComments(int count, int offset)
        {
            var result = await PostApi.GetComments(PostId, count, offset)
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

                    _offset += comments.Count;
                });
            });
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_postIdChangedUpdate)
                return;

            await UpdateComments()
                .ConfigureAwait(true);
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

            --_offset;

            RaiseEvent(new RoutedEventArgs(OnCommentDeleted));
        }
    }
}

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
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(CommentsList),
                new PropertyMetadata(-1, PostIdChangedCallback));
        public static readonly DependencyProperty CommentsCountProperty =
            DependencyProperty.Register(nameof(CommentsCount), typeof(int), typeof(CommentsList),
                new PropertyMetadata(0));

        private const int OffsetPerTime = 20;

        private int _offset;

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
        public int CommentsCount
        {
            get
            {
                return (int)GetValue(CommentsCountProperty);
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

        public async Task UpdateComments()
        {
            lstComments.Children.Clear();

            _offset = 0;

            await LoadNewComments()
                .ConfigureAwait(true);

            PostOverlayPage page = this.TryFindParent<PostOverlayPage>();

            page?.svPost.ScrollToVerticalOffset(0.0);
        }

        public Task LoadNewComments()
        {
            return LoadNewComments(_offset);
        }
        public async Task LoadNewComments(int offset)
        {
            var result = await PostApi.GetComments(PostId, OffsetPerTime, offset)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", "Cannot load comments")
                    .ConfigureAwait(true);
                return;
            }

            await AddComments(result.data)
                .ConfigureAwait(true);
        }

        public Task AddComments(List<CommentSchema> comments)
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

                        lstComments.Children.Insert(0, commentWidget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    if (lstComments.Children.Count >= CommentsCount - 1)
                        btnLoadMore.Visibility = Visibility.Collapsed;

                    _offset += OffsetPerTime;
                });
            });
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateComments()
                .ConfigureAwait(true);
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        private static async void PostIdChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommentsList commentsList = d as CommentsList;

            if (commentsList == null)
                return;

            await commentsList.UpdateComments()
                .ConfigureAwait(true);
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        private async void btnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            btnLoadMore.IsEnabled = false;

            await LoadNewComments()
                .ConfigureAwait(true);

            btnLoadMore.IsEnabled = true;
        }
    }
}

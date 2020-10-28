using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Dialogs;

namespace Memenim.Widgets
{
    public partial class CommentsList : UserControl
    {
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register("PostId", typeof(int), typeof(CommentsList), new PropertyMetadata(-1));
        public static readonly DependencyProperty CommentsCountProperty =
            DependencyProperty.Register("CommentsCount", typeof(int), typeof(CommentsList), new PropertyMetadata(0));

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

        public void AddComments(List<CommentData> comments)
        {
            foreach (var comment in comments)
            {
                UserComment commentWidget = new UserComment
                {
                    CurrentCommentData = comment
                };

                lstComments.Items.Insert(0, commentWidget);
            }

            if (lstComments.Items.Count >= CommentsCount - 1)
                btnLoadMore.Visibility = Visibility.Collapsed;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            lstComments.Items.Clear();

            var result = await PostApi.GetComments(PostId)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            AddComments(result.data);

            _offset += OffsetPerTime;
        }

        private async void btnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            btnLoadMore.IsEnabled = false;

            var result = await PostApi.GetComments(PostId, _offset)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", "Cannot load comments")
                    .ConfigureAwait(true);
                return;
            }

            AddComments(result.data);

            _offset += OffsetPerTime;

            btnLoadMore.IsEnabled = true;
        }
    }
}

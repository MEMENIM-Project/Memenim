using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : PageContent
    {
        public PostOverlayViewModel ViewModel
        {
            get
            {
                return DataContext as PostOverlayViewModel;
            }
        }
        public bool CommentsIsOpen
        {
            get
            {
                return ViewModel.CurrentPostData?.open_comments == 1;
            }
        }

        public PostOverlayPage()
        {
            InitializeComponent();
            DataContext = new PostOverlayViewModel();
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!CommentsIsOpen)
            {
                wdgWriteComment.Visibility = Visibility.Collapsed;
                wdgCommentsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                wdgCommentsList.Visibility = Visibility.Visible;
                wdgWriteComment.Visibility = Visibility.Visible;
            }

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            var result = await UserApi.GetProfileById(SettingsManager.PersistentSettings.CurrentUserId)
                .ConfigureAwait(true);

            if (result.data != null)
                wdgWriteComment.UserAvatarSource = result.data.photo;

            svPost.ScrollToVerticalOffset(0.0);
        }

        private void SvPost_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.VerticalOffset;
        }

        private void CommentsList_CommentDelete(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            --ViewModel.CurrentPostData.comments.count;
            --ViewModel.CurrentPostData.comments.my;
        }

        private void WriteComment_CommentAdd(object sender, RoutedEventArgs e)
        {
            WriteComment writeComment = sender as WriteComment;

            if (writeComment == null)
                return;

            ++ViewModel.CurrentPostData.comments.count;
            ++ViewModel.CurrentPostData.comments.my;
        }
    }
}

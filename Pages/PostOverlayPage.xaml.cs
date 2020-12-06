using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Storage;
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

        public Task UpdatePost()
        {
            return UpdatePost(ViewModel.CurrentPostData.id);
        }
        public async Task UpdatePost(int id)
        {
            ViewModel.CurrentPostData.id = id;

            ViewModel.CurrentPostData = await GetUpdatedPostData()
                .ConfigureAwait(true);
        }

        public async Task<PostSchema> GetUpdatedPostData()
        {
            PostSchema postData = null;

            Dispatcher.Invoke(() =>
            {
                postData = ViewModel.CurrentPostData;
            });

            if (postData == null)
                return postData;

            var result = await PostApi.GetById(
                    postData.id)
                .ConfigureAwait(true);

            if (result.error)
                return postData;

            return result.data ?? postData;
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

            await UpdatePost()
                .ConfigureAwait(true);

            var result = await UserApi.GetProfileById(
                    SettingsManager.PersistentSettings.CurrentUserId)
                .ConfigureAwait(true);

            if (result.data != null)
                wdgWriteComment.SetRealUserAvatarSource(result.data.photo);

            svPost.ScrollToVerticalOffset(0.0);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            base.OnExit(sender, e);
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        protected override async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            base.OnDataContextChanged(sender, e);

            if (e.OldValue is PostOverlayViewModel oldViewModel)
            {
                await StorageManager.SetPostCommentDraft(
                        SettingsManager.PersistentSettings.CurrentUserId,
                        oldViewModel.CurrentPostData.id,
                        wdgWriteComment.CommentText,
                        wdgWriteComment.IsAnonymous)
                    .ConfigureAwait(true);
            }

            wdgWriteComment.CommentText = string.Empty;
            wdgWriteComment.IsAnonymous = false;

            if (e.NewValue is PostOverlayViewModel newViewModel)
            {
                var draft = await StorageManager.GetPostCommentDraft(
                        SettingsManager.PersistentSettings.CurrentUserId,
                        newViewModel.CurrentPostData.id)
                    .ConfigureAwait(true);

                wdgWriteComment.CommentText = draft.CommentText;
                wdgWriteComment.IsAnonymous = draft.IsAnonymous;

                wdgWriteComment.txtContent.CaretIndex = draft.CommentText.Length;
                wdgWriteComment.txtContent.ScrollToEnd();
            }
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        protected override async void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ViewModelPropertyChanged(sender, e);

            if (e.PropertyName.Length == 0)
            {
                await UpdatePost()
                    .ConfigureAwait(true);
            }
        }

        private void SvPost_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.VerticalOffset;
        }

        private async void CommentsList_CommentsUpdate(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            var postData = await GetUpdatedPostData()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.CurrentPostData.comments = postData.comments;
            });
        }

        private async void CommentsList_CommentReply(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            UserComment comment = e.OriginalSource as UserComment;

            if (comment == null)
                return;

            double verticalOffset = svPost.VerticalOffset;
            double extentHeight = svPost.ScrollableHeight;
            string replyText = //$">>> {commentsList.PostId}@{comment.CurrentCommentData.id}@{comment.CurrentCommentData.user.id} {comment.CurrentCommentData.user.name}:\n\n"
                               $">>> {comment.CurrentCommentData.user.name}:\n\n"
                               + $"{comment.CurrentCommentData.text}\n\n"
                               + ">>>\n\n";

            int oldCaretIndex = wdgWriteComment.txtContent.CaretIndex;
            wdgWriteComment.CommentText = replyText + wdgWriteComment.CommentText;
            wdgWriteComment.txtContent.CaretIndex = replyText.Length + oldCaretIndex;
            wdgWriteComment.txtContent.ScrollToEnd();
            wdgWriteComment.txtContent.Focus();

            if (verticalOffset >= extentHeight)
                svPost?.ScrollToEnd();

            if (comment.CurrentCommentData.user.id == SettingsManager.PersistentSettings.CurrentUserId)
                return;

            for (int i = 0; i < 2; ++i)
            {
                if (comment.CurrentCommentData.likes.my == 0)
                {
                    await PostApi.AddLikeComment(
                            SettingsManager.PersistentSettings.CurrentUserToken,
                            comment.CurrentCommentData.id)
                        .ConfigureAwait(true);
                }
                else
                {
                    await PostApi.RemoveLikeComment(
                            SettingsManager.PersistentSettings.CurrentUserToken,
                            comment.CurrentCommentData.id)
                        .ConfigureAwait(true);
                }

                if (comment.CurrentCommentData.likes.my == 0)
                    ++comment.CurrentCommentData.likes.my;
                else
                    --comment.CurrentCommentData.likes.my;
            }
        }

        private async void CommentsList_CommentDelete(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            var postData = await GetUpdatedPostData()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.CurrentPostData.comments = postData.comments;
            });
        }

        private async void WriteComment_CommentAdd(object sender, RoutedEventArgs e)
        {
            WriteComment writeComment = sender as WriteComment;

            if (writeComment == null)
                return;

            var postData = await GetUpdatedPostData()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.CurrentPostData.comments = postData.comments;
            });

            await wdgCommentsList.LoadNewComments()
                .ConfigureAwait(true);
        }

        private void WriteComment_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double verticalOffset = svPost.VerticalOffset;
            double extentHeight = svPost.ExtentHeight;
            double writeCommentHeight =
                Math.Min(Math.Max(wdgWriteComment.ActualHeight, PostGrid.RowDefinitions[1].MinHeight),
                    ActualHeight * 0.3);

            PostGrid.RowDefinitions[0].Height = new GridLength(
                ActualHeight - writeCommentHeight);
            PostGrid.RowDefinitions[1].Height = new GridLength(writeCommentHeight);

            wdgWriteComment.MaxHeight = writeCommentHeight + 1;

            if (verticalOffset >= extentHeight)
                svPost.ScrollToEnd();

            //svPost.ScrollToVerticalOffset(
            //    svPost.VerticalOffset + (e.NewSize.Height - e.PreviousSize.Height));
        }
    }
}

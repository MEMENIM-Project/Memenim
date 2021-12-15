using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Storage;
using Memenim.Utils;
using Memenim.Widgets;
using RIS.Extensions;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : PageContent
    {
        private double _writeCommentMinHeight;
        private double _writeCommentMaxHeight;

        public PostOverlayViewModel ViewModel
        {
            get
            {
                return DataContext as PostOverlayViewModel;
            }
        }

        public PostOverlayPage()
        {
            InitializeComponent();
            DataContext = new PostOverlayViewModel();

            _writeCommentMinHeight = PostGrid.RowDefinitions[1].MinHeight;

            _writeCommentMaxHeight = (int)(ActualHeight * 0.3);
            _writeCommentMaxHeight += 16 - (((int)_writeCommentMaxHeight - _writeCommentMinHeight) % 16);

            wdgWriteComment.MaxHeight = _writeCommentMaxHeight;

            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
        }

        ~PostOverlayPage()
        {
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
        }

        public Task UpdatePost()
        {
            return UpdatePost(ViewModel.CurrentPostData.Id);
        }
        public async Task UpdatePost(int id)
        {
            if (id < 1)
            {
                if (!NavigationController.Instance.IsCurrentContent<PostOverlayPage>())
                    return;

                NavigationController.Instance.GoBack(true);

                string message = LocalizationUtils.GetLocalized("PostNotFound");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            ViewModel.CurrentPostData.Id = id;

            var result = await PostApi.GetById(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentPostData.Id)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                if (!NavigationController.Instance.IsCurrentContent<PostOverlayPage>())
                    return;

                NavigationController.Instance.GoBack(true);

                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                return;
            }

            if (result.Data == null)
            {
                if (!NavigationController.Instance.IsCurrentContent<PostOverlayPage>())
                    return;

                NavigationController.Instance.GoBack(true);

                string message = LocalizationUtils.GetLocalized("PostNotFound");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            ViewModel.CurrentPostData = result.Data;

            wdgPost.UpdateContextMenus();

            if (ViewModel.SourcePost?.CurrentPostData.Id == ViewModel.CurrentPostData.Id)
            {
                ViewModel.SourcePost?.SetValue(
                    Post.CurrentPostDataProperty,
                    ViewModel.CurrentPostData);
            }
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
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    postData.Id)
                .ConfigureAwait(true);

            if (result.IsError)
                return postData;

            return result.Data ?? postData;
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            svPost.ScrollToVerticalOffset(0.0);

            await UpdatePost()
                .ConfigureAwait(true);

            var draft = await StorageManager.GetPostCommentDraft(
                    SettingsManager.PersistentSettings.CurrentUser.Id,
                    ViewModel.CurrentPostData.Id)
                .ConfigureAwait(true);

            wdgWriteComment.CommentText = draft.CommentText;
            wdgWriteComment.IsAnonymous = draft.IsAnonymous;

            wdgWriteComment.ContentTextBox.ScrollToEnd();
            wdgWriteComment.ContentTextBox.CaretIndex = draft.CommentText.Length;
            wdgWriteComment.ContentTextBox.Focus();

            if (svPost.VerticalOffset >= svPost.ScrollableHeight - 20)
                svPost.ScrollToVerticalOffset(0.0);
        }

        protected override async void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            await StorageManager.SetPostCommentDraft(
                    SettingsManager.PersistentSettings.CurrentUser.Id,
                    ViewModel.CurrentPostData.Id,
                    wdgWriteComment.CommentText,
                    wdgWriteComment.IsAnonymous)
                .ConfigureAwait(true);

            wdgCommentsList.CommentsWrapPanel.Children.Clear();

            wdgCommentsList.UpdateLayout();
            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        protected override async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PostOverlayViewModel oldViewModel)
            {
                if (oldViewModel.SourcePost?.CurrentPostData.Id == oldViewModel.CurrentPostData.Id)
                {
                    oldViewModel.SourcePost?.SetValue(
                        Post.CurrentPostDataProperty,
                        oldViewModel.CurrentPostData);
                }

                await StorageManager.SetPostCommentDraft(
                        SettingsManager.PersistentSettings.CurrentUser.Id,
                        oldViewModel.CurrentPostData.Id,
                        wdgWriteComment.CommentText,
                        wdgWriteComment.IsAnonymous)
                    .ConfigureAwait(true);
            }

            wdgWriteComment.CommentText = string.Empty;
            wdgWriteComment.IsAnonymous = false;

            if (e.NewValue is PostOverlayViewModel newViewModel)
            {
                if (newViewModel.SourcePost?.CurrentPostData.Id == newViewModel.CurrentPostData.Id)
                {
                    newViewModel.SourcePost?.SetValue(
                        Post.CurrentPostDataProperty,
                        newViewModel.CurrentPostData);
                }

                var draft = await StorageManager.GetPostCommentDraft(
                        SettingsManager.PersistentSettings.CurrentUser.Id,
                        newViewModel.CurrentPostData.Id)
                    .ConfigureAwait(true);

                wdgWriteComment.CommentText = draft.CommentText;
                wdgWriteComment.IsAnonymous = draft.IsAnonymous;

                wdgWriteComment.ContentTextBox.ScrollToEnd();
                wdgWriteComment.ContentTextBox.CaretIndex = draft.CommentText.Length;
                wdgWriteComment.ContentTextBox.Focus();
            }

            base.OnDataContextChanged(sender, e);
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
            else if (e.PropertyName == nameof(ViewModel.CurrentPostData))
            {
                if (ViewModel.SourcePost?.CurrentPostData.Id == ViewModel.CurrentPostData.Id)
                {
                    ViewModel.SourcePost?.SetValue(
                        Post.CurrentPostDataProperty,
                        ViewModel.CurrentPostData);
                }
            }
        }

        private async void OnCurrentUserChanged(object sender, UserChangedEventArgs e)
        {
            if (e.NewUser.Id == -1)
                return;

            await UpdatePost()
                .ConfigureAwait(true);

            await StorageManager.SetPostCommentDraft(
                    e.OldUser.Id,
                    ViewModel.CurrentPostData.Id,
                    wdgWriteComment.CommentText,
                    wdgWriteComment.IsAnonymous)
                .ConfigureAwait(true);

            var draft = await StorageManager.GetPostCommentDraft(
                    e.NewUser.Id,
                    ViewModel.CurrentPostData.Id)
                .ConfigureAwait(true);

            if (!string.IsNullOrEmpty(draft.CommentText))
            {
                wdgWriteComment.CommentText = draft.CommentText;
                wdgWriteComment.IsAnonymous = draft.IsAnonymous;

                wdgWriteComment.ContentTextBox.ScrollToEnd();
                wdgWriteComment.ContentTextBox.CaretIndex = draft.CommentText.Length;
                wdgWriteComment.ContentTextBox.Focus();
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
                ViewModel.CurrentPostData.Comments = postData.Comments;
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
            double scrollableHeight = svPost.ScrollableHeight;
            CommentReplyModeType replyType = (CommentReplyModeType)SettingsManager.AppSettings.CommentReplyMode;

            string replyText;

            switch (replyType)
            {
                case CommentReplyModeType.Experimental:
                    replyText =
                        //$">>> {commentsList.PostId}@{comment.CurrentCommentData.Id}@{comment.CurrentCommentData.User.Id} {comment.CurrentCommentData.User.Nickname}:\n\n"
                        $">>> {(string.IsNullOrEmpty(comment.CurrentCommentData.User.Nickname) ? "Unknown" : comment.CurrentCommentData.User.Nickname)}:\n\n"
                        + $"{comment.CurrentCommentData.Text}\n\n"
                        + ">>>\n\n";
                    break;
                case CommentReplyModeType.Legacy:
                default:
                    replyText =
                        $"{(string.IsNullOrEmpty(comment.CurrentCommentData.User.Nickname) ? "Unknown" : comment.CurrentCommentData.User.Nickname)}, ";
                    break;
            }

            int oldCaretIndex = wdgWriteComment.ContentTextBox.CaretIndex;

            wdgWriteComment.CommentText = replyText + wdgWriteComment.CommentText;
            wdgWriteComment.ContentTextBox.CaretIndex = replyText.Length + oldCaretIndex;
            wdgWriteComment.ContentTextBox.ScrollToEnd();
            wdgWriteComment.ContentTextBox.Focus();

            if (verticalOffset >= scrollableHeight - 20)
                svPost.ScrollToEnd();

            if (comment.CurrentCommentData.User.Id.HasValue
                && comment.CurrentCommentData.User.Id == SettingsManager.PersistentSettings.CurrentUser.Id)
            {
                return;
            }

            for (var i = 0; i < 2; ++i)
            {
                if (comment.CurrentCommentData.Likes.MyCount == 0)
                {
                    await PostApi.AddLikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            comment.CurrentCommentData.Id)
                        .ConfigureAwait(true);
                }
                else
                {
                    await PostApi.RemoveLikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            comment.CurrentCommentData.Id)
                        .ConfigureAwait(true);
                }

                if (comment.CurrentCommentData.Likes.MyCount == 0)
                    ++comment.CurrentCommentData.Likes.MyCount;
                else
                    --comment.CurrentCommentData.Likes.MyCount;
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
                ViewModel.CurrentPostData.Comments = postData.Comments;
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
                ViewModel.CurrentPostData.Comments = postData.Comments;
            });

            await wdgCommentsList.LoadNewComments()
                .ConfigureAwait(true);
        }

        private void WriteComment_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var actualHeight = ActualHeight;

            var verticalOffset = svPost.VerticalOffset;
            var scrollableHeight = svPost.ScrollableHeight;
            var writeCommentHeight = Math.Clamp(
                (int)e.NewSize.Height + (((int)e.NewSize.Height).IsEven() ? 0 : 1),
                _writeCommentMinHeight, _writeCommentMaxHeight);
            var scrollViewerHeight = actualHeight - writeCommentHeight;

            writeCommentHeight = Math.Floor(writeCommentHeight);
            scrollViewerHeight = Math.Ceiling(scrollViewerHeight);
            scrollViewerHeight += actualHeight - writeCommentHeight - scrollViewerHeight;

            PostGrid.RowDefinitions[0].Height = new GridLength(
                scrollViewerHeight);
            PostGrid.RowDefinitions[1].Height = new GridLength(
                writeCommentHeight);

            pnlWriteComment.Height = writeCommentHeight;

            if (verticalOffset >= scrollableHeight - 20)
                svPost.ScrollToEnd();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var actualHeight = e.NewSize.Height;

            _writeCommentMaxHeight = (int)(actualHeight * 0.3);
            _writeCommentMaxHeight += 16 - (((int)_writeCommentMaxHeight - _writeCommentMinHeight) % 16);

            wdgWriteComment.MaxHeight = _writeCommentMaxHeight;

            var verticalOffset = svPost.VerticalOffset;
            var scrollableHeight = svPost.ScrollableHeight;
            var writeCommentHeight = Math.Clamp(
                (int)wdgWriteComment.ActualHeight + (((int)wdgWriteComment.ActualHeight).IsEven() ? 0 : 1),
                _writeCommentMinHeight, _writeCommentMaxHeight);
            var scrollViewerHeight = actualHeight - writeCommentHeight;

            writeCommentHeight = Math.Floor(writeCommentHeight);
            scrollViewerHeight = Math.Ceiling(scrollViewerHeight);
            scrollViewerHeight += actualHeight - writeCommentHeight - scrollViewerHeight;

            PostGrid.RowDefinitions[0].Height = new GridLength(
                scrollViewerHeight);
            PostGrid.RowDefinitions[1].Height = new GridLength(
                writeCommentHeight);

            pnlWriteComment.Height = writeCommentHeight;

            if (verticalOffset >= scrollableHeight - 20)
                svPost.ScrollToEnd();
        }
    }
}

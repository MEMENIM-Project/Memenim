using System;
using System.Windows;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;

namespace Memenim.Widgets
{
    public partial class UserComment : WidgetContent
    {
        public static readonly RoutedEvent CommentReplyEvent =
            EventManager.RegisterRoutedEvent(nameof(CommentReply), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(UserComment));
        public static readonly RoutedEvent CommentDeleteEvent =
            EventManager.RegisterRoutedEvent(nameof(CommentDelete), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(UserComment));
        
        

        public static readonly DependencyProperty CurrentCommentDataProperty =
            DependencyProperty.Register(nameof(CurrentCommentData), typeof(CommentSchema), typeof(UserComment),
                new PropertyMetadata(new CommentSchema { User = new CommentUserSchema {Id = -1} }));



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



        public CommentSchema CurrentCommentData
        {
            get
            {
                return (CommentSchema)GetValue(CurrentCommentDataProperty);
            }
            set
            {
                SetValue(CurrentCommentDataProperty, value);
            }
        }



        public UserComment()
        {
            InitializeComponent();
            DataContext = this;
        }



        public void UpdateComment()
        {
            if (!CurrentCommentData.User.Id.HasValue
                || CurrentCommentData.User.Id == -1)
            {
                CurrentCommentData.User.Nickname = "Unknown";

                return;
            }

            if (CurrentCommentData.User.Id == SettingsManager.PersistentSettings.CurrentUser.Id)
            {
                EditButton.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
        }



        protected override void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            UpdateComment();
        }



        private void CopyCommentId_Click(object sender,
            RoutedEventArgs e)
        {
            var id = CurrentCommentData.Id
                .ToString();

            Clipboard.SetText(id);
        }

        private async void CopyCommentText_Click(object sender,
            RoutedEventArgs e)
        {
            var text = CurrentCommentData.Text;

            if (text == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(text);
        }



        private void Reply_Click(object sender,
            RoutedEventArgs e)
        {
            ReplyButton.IsEnabled = false;

            try
            {
                RaiseEvent(new RoutedEventArgs(CommentReplyEvent));
            }
            finally
            {
                ReplyButton.IsEnabled = true;
            }
        }

        private async void Edit_Click(object sender,
            RoutedEventArgs e)
        {
            EditButton.IsEnabled = false;

            try
            {
                if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
                    return;
                if (CurrentCommentData.User.Id != SettingsManager.PersistentSettings.CurrentUser.Id)
                    return;

                var title = LocalizationUtils.GetLocalized("EditingCommentTitle");
                var message = LocalizationUtils.GetLocalized("EditingCommentMessage");

                var oldValue = CurrentCommentData.Text;
                var value = await DialogManager.ShowMultilineTextDialog(
                        title, message, oldValue)
                    .ConfigureAwait(true);

                if (value == null)
                    return;

                var request = await PostApi.EditComment(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentCommentData.Id,
                        value)
                    .ConfigureAwait(true);

                if (request.IsError)
                {
                    await DialogManager.ShowErrorDialog(request.Message)
                        .ConfigureAwait(true);

                    return;
                }

                CurrentCommentData.Text = value;
            }
            finally
            {
                EditButton.IsEnabled = true;
            }
        }

        private async void Delete_Click(object sender,
            RoutedEventArgs e)
        {
            DeleteButton.IsEnabled = false;

            try
            {
                if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
                    return;
                if (CurrentCommentData.User.Id != SettingsManager.PersistentSettings.CurrentUser.Id)
                    return;

                var confirmResult = await DialogManager.ShowConfirmationDialog()
                    .ConfigureAwait(true);

                if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    return;

                var result = await PostApi.RemoveComment(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentCommentData.Id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                Visibility = Visibility.Collapsed;

                RaiseEvent(new RoutedEventArgs(CommentDeleteEvent));
            }
            finally
            {
                DeleteButton.IsEnabled = true;
            }
        }

        private async void Like_Click(object sender,
            RoutedEventArgs e)
        {
            LikeButton.IsEnabled = false;

            try
            {
                ApiResponse<CountSchema> result;

                if (CurrentCommentData.Likes.MyCount == 0)
                {
                    result = await PostApi.AddLikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            CurrentCommentData.Id)
                        .ConfigureAwait(true);
                }
                else
                {
                    result = await PostApi.RemoveLikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            CurrentCommentData.Id)
                        .ConfigureAwait(true);
                }

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (CurrentCommentData.Likes.MyCount == 0)
                    ++CurrentCommentData.Likes.MyCount;
                else
                    --CurrentCommentData.Likes.MyCount;

                CurrentCommentData.Likes.TotalCount = result.Data.Count;
            }
            finally
            {
                LikeButton.IsEnabled = true;
            }
        }

        private async void Dislike_Click(object sender,
            RoutedEventArgs e)
        {
            DislikeButton.IsEnabled = false;

            try
            {
                ApiResponse<CountSchema> result;

                if (CurrentCommentData.Dislikes.MyCount == 0)
                {
                    result = await PostApi.AddDislikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            CurrentCommentData.Id)
                        .ConfigureAwait(true);
                }
                else
                {
                    result = await PostApi.RemoveDislikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            CurrentCommentData.Id)
                        .ConfigureAwait(true);
                }

                if (result.IsError)
                {
                    if (string.Compare(result.Message, "id is not defined", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (CurrentCommentData.Dislikes.MyCount == 0)
                        {
                            ++CurrentCommentData.Dislikes.MyCount;
                            ++CurrentCommentData.Dislikes.TotalCount;
                        }
                        else
                        {
                            --CurrentCommentData.Dislikes.MyCount;
                            --CurrentCommentData.Dislikes.TotalCount;
                        }

                        return;
                    }

                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (CurrentCommentData.Dislikes.MyCount == 0)
                    ++CurrentCommentData.Dislikes.MyCount;
                else
                    --CurrentCommentData.Dislikes.MyCount;

                CurrentCommentData.Dislikes.TotalCount = result.Data.Count;
            }
            finally
            {
                DislikeButton.IsEnabled = true;
            }
        }

        private void Avatar_MouseLeftButtonUp(object sender,
            MouseButtonEventArgs e)
        {
            if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
                return;

            NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
            {
                CurrentProfileData = new ProfileSchema
                {
                    Id = CurrentCommentData.User.Id.Value
                }
            });
        }
    }
}

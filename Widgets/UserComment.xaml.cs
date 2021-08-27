using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        public static readonly RoutedEvent OnCommentReplied =
            EventManager.RegisterRoutedEvent(nameof(CommentReply), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(UserComment));
        public static readonly RoutedEvent OnCommentDeleted =
            EventManager.RegisterRoutedEvent(nameof(CommentDelete), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(UserComment));
        public static readonly DependencyProperty CurrentCommentDataProperty =
            DependencyProperty.Register(nameof(CurrentCommentData), typeof(CommentSchema), typeof(UserComment),
                new PropertyMetadata(new CommentSchema { User = new CommentUserSchema {Id = -1} }));

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

        public readonly Brush AvatarBorderBackground;
        public readonly Brush AvatarBorderDefaultBackground;

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

            AvatarBorderDefaultBackground = (Brush)FindResource(
                "MahApps.Brushes.Gray10");
            AvatarBorderBackground = AvatarBorder.Background;
        }

        public void UpdateComment()
        {
            if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
            {
                CurrentCommentData.User.Nickname = "Unknown";
                return;
            }

            if (CurrentCommentData.User.Id == SettingsManager.PersistentSettings.CurrentUser.Id)
            {
                btnEdit.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateComment();
        }

        private void CopyCommentId_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentCommentData.Id.ToString());
        }

        private void CopyCommentText_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentCommentData.Text);
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            btnReply.IsEnabled = false;

            RaiseEvent(new RoutedEventArgs(OnCommentReplied));

            btnReply.IsEnabled = true;
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            btnEdit.IsEnabled = false;

            if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
                return;

            if (CurrentCommentData.User.Id != SettingsManager.PersistentSettings.CurrentUser.Id)
                return;

            var title = LocalizationUtils.GetLocalized("EditingCommentTitle");
            var message = LocalizationUtils.GetLocalized("EditingCommentMessage");

            string oldValue = CurrentCommentData.Text;
            string value = await DialogManager.ShowMultilineTextDialog(
                    title, message, oldValue)
                .ConfigureAwait(true);

            if (value == null)
            {
                btnEdit.IsEnabled = true;
                return;
            }

            var request = await PostApi.EditComment(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    CurrentCommentData.Id,
                    value)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                btnEdit.IsEnabled = true;
                return;
            }

            CurrentCommentData.Text = value;

            btnEdit.IsEnabled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete.IsEnabled = false;

            if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
                return;

            if (CurrentCommentData.User.Id != SettingsManager.PersistentSettings.CurrentUser.Id)
                return;

            var confirmResult = await DialogManager.ShowConfirmationDialog()
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                btnDelete.IsEnabled = true;
                return;
            }

            var result = await PostApi.RemoveComment(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    CurrentCommentData.Id)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                btnDelete.IsEnabled = true;
                return;
            }

            Visibility = Visibility.Collapsed;

            RaiseEvent(new RoutedEventArgs(OnCommentDeleted));

            btnDelete.IsEnabled = true;
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            stLikes.IsEnabled = false;

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

                stLikes.IsEnabled = true;
                return;
            }

            if (CurrentCommentData.Likes.MyCount == 0)
                ++CurrentCommentData.Likes.MyCount;
            else
                --CurrentCommentData.Likes.MyCount;

            CurrentCommentData.Likes.TotalCount = result.Data.Count;

            stLikes.IsEnabled = true;
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            stDislikes.IsEnabled = false;

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
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                stDislikes.IsEnabled = true;
                return;
            }

            if (CurrentCommentData.Dislikes.MyCount == 0)
                ++CurrentCommentData.Dislikes.MyCount;
            else
                --CurrentCommentData.Dislikes.MyCount;

            CurrentCommentData.Dislikes.TotalCount = result.Data.Count;

            stDislikes.IsEnabled = true;
        }

        private void Avatar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!CurrentCommentData.User.Id.HasValue || CurrentCommentData.User.Id == -1)
                return;

            NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
            {
                CurrentProfileData = new ProfileSchema()
                {
                    Id = CurrentCommentData.User.Id.Value
                }
            });
        }

        private void AvatarImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                AvatarBorder.Background = AvatarBorderDefaultBackground;

                return;
            }

            AvatarBorder.Background = AvatarBorderBackground;
        }
    }
}

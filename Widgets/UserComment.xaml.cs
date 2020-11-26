using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;

namespace Memenim.Widgets
{
    public partial class UserComment : UserControl
    {
        public static readonly RoutedEvent OnCommentDeleted =
            EventManager.RegisterRoutedEvent(nameof(CommentDelete), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(UserComment));
        public static readonly DependencyProperty CurrentCommentDataProperty =
            DependencyProperty.Register(nameof(CurrentCommentData), typeof(CommentSchema), typeof(UserComment),
                new PropertyMetadata(new CommentSchema { user = new CommentUserSchema {id = -1} }));

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
            if (CurrentCommentData.user.id == -1)
            {
                CurrentCommentData.user.name = "Unknown";
                return;
            }

            if (CurrentCommentData.user.id == SettingsManager.PersistentSettings.CurrentUserId)
            {
                brdActions.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateComment();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete.IsEnabled = false;

            var confirmResult = await DialogManager.ShowDialog("Confirmation", "Are you sure?",
                    MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative)
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                btnDelete.IsEnabled = true;
                return;
            }

            var result = await PostApi.RemoveComment(
                    SettingsManager.PersistentSettings.CurrentUserToken,
                    CurrentCommentData.id)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
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

            if (CurrentCommentData.likes.my == 0)
            {
                result = await PostApi.AddLikeComment(SettingsManager.PersistentSettings.CurrentUserToken, CurrentCommentData.id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveLikeComment(SettingsManager.PersistentSettings.CurrentUserToken, CurrentCommentData.id)
                    .ConfigureAwait(true);
            }

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                stLikes.IsEnabled = true;
                return;
            }

            if (CurrentCommentData.likes.my == 0)
                ++CurrentCommentData.likes.my;
            else
                --CurrentCommentData.likes.my;

            CurrentCommentData.likes.count = result.data.count;

            stLikes.IsEnabled = true;
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            stDislikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentCommentData.dislikes.my == 0)
            {
                result = await PostApi.AddDislikeComment(SettingsManager.PersistentSettings.CurrentUserToken, CurrentCommentData.id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveDislikeComment(SettingsManager.PersistentSettings.CurrentUserToken, CurrentCommentData.id)
                    .ConfigureAwait(true);
            }

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                stDislikes.IsEnabled = true;
                return;
            }

            if (CurrentCommentData.dislikes.my == 0)
                ++CurrentCommentData.dislikes.my;
            else
                --CurrentCommentData.dislikes.my;

            CurrentCommentData.dislikes.count = result.data.count;

            stDislikes.IsEnabled = true;
        }

        private void Avatar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentCommentData.user.id == -1)
                return;

            NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
            {
                CurrentProfileData = new ProfileSchema()
                {
                    id = CurrentCommentData.user.id
                }
            });
        }
    }
}

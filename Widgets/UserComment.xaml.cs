using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Pages;
using Memenim.Core.Api;
using Memenim.Core.Data;

namespace Memenim.Widgets
{
    public partial class UserComment : UserControl
    {
        public static readonly DependencyProperty CurrentCommentDataProperty =
            DependencyProperty.Register("CurrentCommentData", typeof(CommentData), typeof(UserComment),
                new PropertyMetadata(new CommentData {user = new CommentData.CommentUserData {id = -1} }));

        public CommentData CurrentCommentData
        {
            get
            {
                return (CommentData)GetValue(CurrentCommentDataProperty);
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
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateComment();
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            var result = await PostApi.AddLikeComment(CurrentCommentData.id, AppPersistent.UserToken)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            //++Likes;
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            var result = await PostApi.AddDislikeComment(CurrentCommentData.id, AppPersistent.UserToken)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            //++Dislikes;
        }

        private void Avatar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentCommentData.user.id == -1)
                return;

            PageNavigationManager.SwitchToSubPage(new UserProfilePage
            {
                CurrentProfileData = new ProfileData
                {
                    id = CurrentCommentData.user.id
                }
            });
            PageNavigationManager.CloseOverlay();
        }
    }
}

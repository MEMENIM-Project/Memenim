using System;
using System.Windows;
using System.Windows.Input;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;

namespace Memenim.Widgets
{
    public partial class CommentInfo : WidgetContent
    {
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register(nameof(UserId), typeof(int), typeof(CommentInfo),
                new PropertyMetadata(-1));
        public static readonly DependencyProperty UserNicknameProperty =
            DependencyProperty.Register(nameof(UserNickname), typeof(string), typeof(CommentInfo),
                new PropertyMetadata("Unknown"));
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource), typeof(string), typeof(CommentInfo),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty UtcDateProperty =
            DependencyProperty.Register(nameof(UtcDate), typeof(ulong), typeof(CommentInfo),
                new PropertyMetadata(0UL));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register(nameof(IsAnonymous), typeof(bool), typeof(CommentInfo),
                new PropertyMetadata(false));



        public int UserId
        {
            get
            {
                return (int)GetValue(UserIdProperty);
            }
            set
            {
                SetValue(UserIdProperty, value);
            }
        }
        public string UserNickname
        {
            get
            {
                return (string)GetValue(UserNicknameProperty);
            }
            set
            {
                SetValue(UserNicknameProperty, value);
            }
        }
        public string UserAvatarSource
        {
            get
            {
                return (string)GetValue(UserAvatarSourceProperty);
            }
            set
            {
                SetValue(UserAvatarSourceProperty, value);
            }
        }
        public ulong UtcDate
        {
            get
            {
                return (ulong)GetValue(UtcDateProperty);
            }
            set
            {
                SetValue(UtcDateProperty, value);
            }
        }
        public bool IsAnonymous
        {
            get
            {
                return (bool)GetValue(IsAnonymousProperty);
            }
            set
            {
                SetValue(IsAnonymousProperty, value);
            }
        }



        public CommentInfo()
        {
            InitializeComponent();
            DataContext = this;
        }



        private void User_MouseLeftButtonUp(object sender,
            MouseButtonEventArgs e)
        {
            if (UserId == -1)
                return;

            NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
            {
                CurrentProfileData = new ProfileSchema
                {
                    Id = UserId
                }
            });

            e.Handled = true;
        }
    }
}

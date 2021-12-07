using System;
using System.Windows;
using System.Windows.Input;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;

namespace Memenim.Widgets
{
    public partial class PostInfo : WidgetContent
    {
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register(nameof(UserId), typeof(int), typeof(PostInfo),
                new PropertyMetadata(-1));
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(PostInfo),
                new PropertyMetadata("Unknown"));
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource), typeof(string), typeof(PostInfo),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty UtcDateProperty =
            DependencyProperty.Register(nameof(UtcDate), typeof(ulong), typeof(PostInfo),
                new PropertyMetadata(0UL));
        public static readonly DependencyProperty PostStatusProperty =
            DependencyProperty.Register(nameof(PostStatus), typeof(PostStatusType), typeof(PostInfo),
                new PropertyMetadata(PostStatusType.Published));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register(nameof(IsAnonymous), typeof(bool), typeof(PostInfo),
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
        public string UserName
        {
            get
            {
                return (string)GetValue(UserNameProperty);
            }
            set
            {
                SetValue(UserNameProperty, value);
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
        public PostStatusType PostStatus
        {
            get
            {
                return (PostStatusType)GetValue(PostStatusProperty);
            }
            set
            {
                SetValue(PostStatusProperty, value);
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



        public PostInfo()
        {
            InitializeComponent();
            DataContext = this;
        }



        private void Avatar_MouseLeftButtonUp(object sender,
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

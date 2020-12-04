using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Converters;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;

namespace Memenim.Widgets
{
    public partial class PosterBanner : UserControl
    {
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register(nameof(UserId), typeof(int), typeof(PosterBanner),
                new PropertyMetadata(-1));
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(PosterBanner),
                new PropertyMetadata("Unknown"));
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource), typeof(string), typeof(PosterBanner),
                new PropertyMetadata((string) null));
        public static readonly DependencyProperty PostTimeProperty =
            DependencyProperty.Register(nameof(PostTime), typeof(string), typeof(PosterBanner),
                new PropertyMetadata(DateTime.MinValue.ToString(CultureInfo.CurrentCulture)));
        public static readonly DependencyProperty PostStatusValueProperty =
            DependencyProperty.Register(nameof(PostStatusValue), typeof(int), typeof(PosterBanner),
                new PropertyMetadata(0));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register(nameof(IsAnonymous), typeof(bool), typeof(PosterBanner),
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
        public string PostTime
        {
            get
            {
                return (string)GetValue(PostTimeProperty);
            }
            set
            {
                SetValue(PostTimeProperty, value);
            }
        }
        public int PostStatusValue
        {
            get
            {
                return (int)GetValue(PostStatusValueProperty);
            }
            set
            {
                SetValue(PostStatusValueProperty, value);
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

        public PosterBanner()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OnPoster_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (UserId == -1)
                return;

            NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
            {
                CurrentProfileData = new ProfileSchema
                {
                    id = UserId
                }
            });

            e.Handled = true;
        }
    }
}

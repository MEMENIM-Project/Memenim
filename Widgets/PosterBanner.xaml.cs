﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;

namespace Memenim.Widgets
{
    public partial class PosterBanner : WidgetContent
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
        public static readonly DependencyProperty UtcDateProperty =
            DependencyProperty.Register(nameof(UtcDate), typeof(ulong), typeof(PosterBanner),
                new PropertyMetadata(0UL));
        public static readonly DependencyProperty PostStatusProperty =
            DependencyProperty.Register(nameof(PostStatus), typeof(PostStatusType), typeof(PosterBanner),
                new PropertyMetadata(PostStatusType.Published));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register(nameof(IsAnonymous), typeof(bool), typeof(PosterBanner),
                new PropertyMetadata(false));

        public readonly Brush AvatarBorderBackground;
        public readonly Brush AvatarBorderDefaultBackground;

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

        public PosterBanner()
        {
            InitializeComponent();
            DataContext = this;

            AvatarBorderDefaultBackground = (Brush)FindResource(
                "MahApps.Brushes.Gray10");
            AvatarBorderBackground = AvatarBorder.Background;
        }

        private void Avatar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

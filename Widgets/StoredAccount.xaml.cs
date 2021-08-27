﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Settings.Entities;

namespace Memenim.Widgets
{
    public partial class StoredAccount : WidgetContent
    {
        public static readonly RoutedEvent OnAccountClicked =
            EventManager.RegisterRoutedEvent(nameof(AccountClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(StoredAccount));
        public static readonly RoutedEvent OnAccountDeleted =
            EventManager.RegisterRoutedEvent(nameof(AccountDelete), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(StoredAccount));
        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register(nameof(Account), typeof(User), typeof(StoredAccount),
                new PropertyMetadata(new User(null, null, -1, null, UserStoreType.Unknown)));
        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(StoredAccount),
                new PropertyMetadata("Unknown"));
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource), typeof(string), typeof(StoredAccount),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty UserStatusProperty =
            DependencyProperty.Register(nameof(UserStatus), typeof(UserStatusType), typeof(StoredAccount),
                new PropertyMetadata(UserStatusType.Active));

        public event EventHandler<RoutedEventArgs> AccountClick
        {
            add
            {
                AddHandler(OnAccountClicked, value);
            }
            remove
            {
                RemoveHandler(OnAccountClicked, value);
            }
        }
        public event EventHandler<RoutedEventArgs> AccountDelete
        {
            add
            {
                AddHandler(OnAccountDeleted, value);
            }
            remove
            {
                RemoveHandler(OnAccountDeleted, value);
            }
        }

        public readonly Brush AvatarBorderBackground;
        public readonly Brush AvatarBorderDefaultBackground;

        public User Account
        {
            get
            {
                return (User)GetValue(AccountProperty);
            }
            set
            {
                SetValue(AccountProperty, value);
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
        public UserStatusType UserStatus
        {
            get
            {
                return (UserStatusType)GetValue(UserStatusProperty);
            }
            set
            {
                SetValue(UserStatusProperty, value);
            }
        }

        public StoredAccount()
        {
            InitializeComponent();
            DataContext = this;

            AvatarBorderDefaultBackground = (Brush)FindResource(
                "MahApps.Brushes.Gray10");
            AvatarBorderBackground = AvatarBorder.Background;
        }

        public async Task UpdateAccount()
        {
            if (Account.Id == -1)
                return;

            var result = await UserApi.GetProfileById(Account.Id)
                .ConfigureAwait(true);

            if (result.IsError || result.Data == null)
                return;

            UserName = result.Data.Nickname;
            UserAvatarSource = result.Data.PhotoUrl;
            UserStatus = (UserStatusType)((byte)result.Data.Status);
        }

        public async Task UpdateStatus()
        {
            if (Account.Id == -1)
                return;

            var result = await UserApi.GetProfileById(Account.Id)
                .ConfigureAwait(true);

            if (result.IsError || result.Data == null)
                return;

            UserStatus = (UserStatusType)((byte)result.Data.Status);
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateAccount()
                .ConfigureAwait(true);
        }

        private void Account_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnAccountClicked));
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete.IsEnabled = false;

            var confirmResult = await DialogManager.ShowConfirmationDialog()
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                btnDelete.IsEnabled = true;
                return;
            }

            Visibility = Visibility.Collapsed;

            RaiseEvent(new RoutedEventArgs(OnAccountDeleted));

            btnDelete.IsEnabled = true;
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

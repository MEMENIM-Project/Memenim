using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Settings.Entities;

namespace Memenim.Widgets
{
    public partial class StoredAccount : WidgetContent
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(StoredAccount));
        public static readonly RoutedEvent AccountDeleteEvent =
            EventManager.RegisterRoutedEvent(nameof(AccountDelete), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(StoredAccount));
        

        
        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register(nameof(UserAccount), typeof(User), typeof(StoredAccount),
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



        public event EventHandler<RoutedEventArgs> Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
            }
        }
        public event EventHandler<RoutedEventArgs> AccountDelete
        {
            add
            {
                AddHandler(AccountDeleteEvent, value);
            }
            remove
            {
                RemoveHandler(AccountDeleteEvent, value);
            }
        }



        public User UserAccount
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
        }



        public async Task UpdateAccount()
        {
            if (UserAccount.Id == -1)
                return;

            var result = await UserApi.GetProfileById(
                    UserAccount.Id)
                .ConfigureAwait(true);

            if (result.IsError || result.Data == null)
                return;

            UserName = result.Data.Nickname;
            UserAvatarSource = result.Data.PhotoUrl;
            UserStatus = result.Data.Status;
        }

        public async Task UpdateStatus()
        {
            if (UserAccount.Id == -1)
                return;

            var result = await UserApi.GetProfileById(
                    UserAccount.Id)
                .ConfigureAwait(true);

            if (result.IsError || result.Data == null)
                return;

            UserStatus = result.Data.Status;
        }



        protected override async void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await UpdateAccount()
                .ConfigureAwait(true);
        }



        private void Account_Click(object sender,
            RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        private async void Delete_Click(object sender,
            RoutedEventArgs e)
        {
            DeleteButton.IsEnabled = false;

            try
            {
                var confirmResult = await DialogManager.ShowConfirmationDialog()
                    .ConfigureAwait(true);

                if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    return;

                Visibility = Visibility.Collapsed;

                RaiseEvent(new RoutedEventArgs(AccountDeleteEvent));
            }
            finally
            {
                DeleteButton.IsEnabled = true;
            }
        }
    }
}

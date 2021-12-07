using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Settings;
using Memenim.Utils;

namespace Memenim.Widgets
{
    public partial class WriteComment : WidgetContent
    {
        public static readonly RoutedEvent CommentAddEvent =
            EventManager.RegisterRoutedEvent(nameof(CommentAdd), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(WriteComment));
        
        
        
        public static readonly DependencyProperty CommentTextProperty =
            DependencyProperty.Register(nameof(CommentText), typeof(string), typeof(WriteComment),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(WriteComment),
                new PropertyMetadata(-1));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register(nameof(IsAnonymous), typeof(bool), typeof(WriteComment),
                new PropertyMetadata(false, IsAnonymousChangedCallback));



        public event EventHandler<RoutedEventArgs> CommentAdd
        {
            add
            {
                AddHandler(CommentAddEvent, value);
            }
            remove
            {
                RemoveHandler(CommentAddEvent, value);
            }
        }



        public string CommentText
        {
            get
            {
                Focus();

                var commentText =
                    (string)GetValue(CommentTextProperty);

                ContentTextBox.Focus();

                return commentText;
            }
            set
            {
                SetValue(CommentTextProperty, value);
            }
        }
        public int PostId
        {
            get
            {
                return (int)GetValue(PostIdProperty);
            }
            set
            {
                SetValue(PostIdProperty, value);
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
        private string _userAvatarSource;
        public string UserAvatarSource
        {
            get
            {
                return _userAvatarSource;
            }
            set
            {
                _userAvatarSource = value;
                OnPropertyChanged(nameof(UserAvatarSource));

                UpdateDisplayedAvatar();
            }
        }
        private string _displayedUserAvatarSource;
        public string DisplayedUserAvatarSource
        {
            get
            {
                return _displayedUserAvatarSource;
            }
            set
            {
                _displayedUserAvatarSource = value;
                OnPropertyChanged(nameof(DisplayedUserAvatarSource));
            }
        }



        public WriteComment()
        {
            InitializeComponent();
            DataContext = this;

            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
            ProfileUtils.AvatarChanged += OnAvatarChanged;
        }

        ~WriteComment()
        {
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
            ProfileUtils.AvatarChanged -= OnAvatarChanged;
        }



        private static void IsAnonymousChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as WriteComment;

            target?.OnIsAnonymousChanged(e);
        }



        private void OnIsAnonymousChanged(
            DependencyPropertyChangedEventArgs e)
        {
            SendAnonymouslyButton.IsChecked =
                (bool)e.NewValue;

            UpdateDisplayedAvatar();
        }



        private async Task UpdateAvatarSource(
            int userId)
        {
            var result = await UserApi.GetProfileById(
                    userId)
                .ConfigureAwait(true);

            if (result.IsError
                || result.Data == null)
            {
                UserAvatarSource = null;

                return;
            }

            UserAvatarSource = result.Data
                .PhotoUrl;
        }

        private void UpdateDisplayedAvatar()
        {
            DisplayedUserAvatarSource = !IsAnonymous
                ? UserAvatarSource
                : null;
        }



        protected override async void OnCreated(object sender,
            EventArgs e)
        {
            await UpdateAvatarSource(
                    SettingsManager.PersistentSettings.CurrentUser.Id)
                .ConfigureAwait(false);
        }



        private async void OnCurrentUserChanged(object sender,
            UserChangedEventArgs e)
        {
            await UpdateAvatarSource(
                    e.NewUser.Id)
                .ConfigureAwait(false);
        }

        private void OnAvatarChanged(object sender,
            UserPhotoChangedEventArgs e)
        {
            UserAvatarSource = e.NewPhoto;
        }



        private void ContentTextBox_KeyUp(object sender,
            KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Enter)
            {
                if (SendButton.IsEnabled)
                {
                    SendButton.Focus();
                    SendButton_Click(this, new RoutedEventArgs());
                }
            }
        }

        private async void SendButton_Click(object sender,
            RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;

            SendButton.Focus();

            var result = await PostApi.AddComment(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    PostId, ContentTextBox.Text, IsAnonymous)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                SendButton.IsEnabled = true;
                return;
            }

            ContentTextBox.Text = string.Empty;

            RaiseEvent(new RoutedEventArgs(CommentAddEvent));

            ContentTextBox.Focus();

            SendButton.IsEnabled = true;
        }

        private async void SendAnonymouslyButton_Click(object sender,
            RoutedEventArgs e)
        {
            if (!SendAnonymouslyButton.IsChecked)
            {
                IsAnonymous = false;

                return;
            }

            var additionalMessage = LocalizationUtils
                .GetLocalized("YouMaybeBannedConfirmationMessage");
            var confirmResult = await DialogManager.ShowConfirmationDialog(
                    additionalMessage)
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                SendAnonymouslyButton.IsChecked = false;

                Keyboard.ClearFocus();

                return;
            }

            IsAnonymous = true;
        }
    }
}

using System;
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
        public static readonly RoutedEvent OnCommentAdded =
            EventManager.RegisterRoutedEvent(nameof(CommentAdd), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(WriteComment));
        public static readonly DependencyProperty CommentTextProperty =
            DependencyProperty.Register(nameof(CommentText), typeof(string), typeof(WriteComment),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource), typeof(string), typeof(WriteComment),
                new PropertyMetadata((string) null));
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
                AddHandler(OnCommentAdded, value);
            }
            remove
            {
                RemoveHandler(OnCommentAdded, value);
            }
        }

        private string _realUserAvatarSource;

        public string CommentText
        {
            get
            {
                Focus();

                string commentText = (string)GetValue(CommentTextProperty);

                txtContent.Focus();

                return commentText;
            }
            set
            {
                SetValue(CommentTextProperty, value);
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

        public WriteComment()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetRealUserAvatarSource(string source)
        {
            _realUserAvatarSource = source;

            if (!IsAnonymous)
                UserAvatarSource = source;
        }

        private static void IsAnonymousChangedCallback(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            WriteComment writeComment = d as WriteComment;

            if (writeComment == null)
                return;

            writeComment.btnSendAnonymously.IsChecked = writeComment.IsAnonymous;

            if (!writeComment.IsAnonymous)
            {
                writeComment.UserAvatarSource = writeComment._realUserAvatarSource;
            }
            else
            {
                writeComment._realUserAvatarSource = writeComment.UserAvatarSource;
                writeComment.UserAvatarSource = null;
            }
        }

        private void txtContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Enter)
            {
                if (btnSend.IsEnabled)
                {
                    btnSend.Focus();
                    btnSend_Click(this, new RoutedEventArgs());
                }
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            btnSend.IsEnabled = false;

            btnSend.Focus();

            var result = await PostApi.AddComment(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    PostId, txtContent.Text, IsAnonymous)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                btnSend.IsEnabled = true;
                return;
            }

            txtContent.Text = string.Empty;

            RaiseEvent(new RoutedEventArgs(OnCommentAdded));

            txtContent.Focus();

            btnSend.IsEnabled = true;
        }

        private async void btnSendAnonymously_Click(object sender, RoutedEventArgs e)
        {
            if (!btnSendAnonymously.IsChecked)
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
                btnSendAnonymously.IsChecked = false;

                Keyboard.ClearFocus();

                return;
            }

            IsAnonymous = true;
        }
    }
}

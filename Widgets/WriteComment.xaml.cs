using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Widgets
{
    public partial class WriteComment : UserControl
    {
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register("UserAvatarSource", typeof(string), typeof(WriteComment), new PropertyMetadata((string) null));
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register("PostId", typeof(int), typeof(WriteComment), new PropertyMetadata(-1));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register("IsAnonymous", typeof(bool), typeof(WriteComment), new PropertyMetadata(false));

        private string _realUserAvatarSource;

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

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostApi.AddComment(PostId, txtContent.Text, IsAnonymous, SettingsManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (!res.error)
            {
                await DialogManager.ShowDialog("S U C C", "Comment sent")
                    .ConfigureAwait(true);
            }
            else
            {
                await DialogManager.ShowDialog("F U C K", res.message)
                    .ConfigureAwait(true);
            }

            txtContent.Text = string.Empty;
        }

        private void btnAnonymous_Click(object sender, RoutedEventArgs e)
        {
            IsAnonymous = !IsAnonymous;

            if (!IsAnonymous)
            {
                UserAvatarSource = _realUserAvatarSource;
            }
            else
            {
                _realUserAvatarSource = UserAvatarSource;
                UserAvatarSource = null;
            }
        }
    }
}

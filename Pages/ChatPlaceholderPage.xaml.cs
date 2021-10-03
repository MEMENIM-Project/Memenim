using System;
using System.Diagnostics;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Generating;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;

namespace Memenim.Pages
{
    public partial class ChatPlaceholderPage : PageContent
    {
        public ChatPlaceholderViewModel ViewModel
        {
            get
            {
                return DataContext as ChatPlaceholderViewModel;
            }
        }

        public ChatPlaceholderPage()
        {
            InitializeComponent();
            DataContext = new ChatPlaceholderViewModel();
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            txtSmile.Text = GeneratingManager.GetRandomSmile();
        }

        private async void CopyLogin_Click(object sender, RoutedEventArgs e)
        {
            btnCopyLogin.IsEnabled = false;
            btnCopyPassword.IsEnabled = false;

            var login = SettingsManager.PersistentSettings.CurrentUser.Login;

            if (login == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(login);

            btnCopyLogin.IsEnabled = true;
            btnCopyPassword.IsEnabled = true;
        }

        private async void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            btnCopyLogin.IsEnabled = false;
            btnCopyPassword.IsEnabled = false;

            try
            {
                if (SettingsManager.PersistentSettings.CurrentUser.HasStoredRocketPassword())
                {
                    Clipboard.SetText(SettingsManager.PersistentSettings.CurrentUser.RocketPassword);

                    return;
                }

                var result = await UserApi.GetRocketPassword(
                        SettingsManager.PersistentSettings.CurrentUser.Token)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                var password = result.Data.Password;

                if (password == null)
                {
                    var message = LocalizationUtils
                        .GetLocalized("CopyingToClipboardErrorMessage");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                SettingsManager.PersistentSettings.CurrentUser.SetRocketPassword(
                    password);

                Clipboard.SetText(password);
            }
            finally
            {
                btnCopyLogin.IsEnabled = true;
                btnCopyPassword.IsEnabled = true;
            }
        }

        private void GoToChatWebPage_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "https://chat.apianon.ru/",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }
}

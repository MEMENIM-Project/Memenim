using System;
using System.Diagnostics;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Generating;
using Memenim.Pages.ViewModel;
using Memenim.Settings;

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

        private void CopyLogin_Click(object sender, RoutedEventArgs e)
        {
            btnCopyLogin.IsEnabled = false;
            btnCopyPassword.IsEnabled = false;

            Clipboard.SetText(SettingsManager.PersistentSettings.CurrentUser.Login);

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

                SettingsManager.PersistentSettings.CurrentUser.SetRocketPassword(
                    result.Data.Password);

                Clipboard.SetText(result.Data.Password);
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

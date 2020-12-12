using System;
using System.Diagnostics;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Pages.ViewModel;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class ChatPlaceholderPage : PageContent
    {
        private static readonly Random Random = new Random();
        private static readonly string[] Smiles =
        {
            "(ﾟдﾟ；)",
            "(ó﹏ò｡)",
            "(´ω｀*)",
            "(┛ಠДಠ)┛彡┻━┻",
            "(* _ω_)…"
        };

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
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);
            txtSmile.Text = Smiles[Random.Next(0, Smiles.Length - 1)];
        }

        private void CopyLogin_Click(object sender, RoutedEventArgs e)
        {
            btnCopyLogin.IsEnabled = false;

            Clipboard.SetText(SettingsManager.PersistentSettings.CurrentUserLogin);

            btnCopyLogin.IsEnabled = true;
        }

        private async void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            btnCopyPassword.IsEnabled = false;

            var result = await UserApi.GetRocketPassword(
                    SettingsManager.PersistentSettings.CurrentUserToken)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                btnCopyPassword.IsEnabled = true;
                return;
            }

            Clipboard.SetText(result.data.password);

            btnCopyPassword.IsEnabled = true;
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

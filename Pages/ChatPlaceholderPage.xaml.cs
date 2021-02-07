using System;
using System.Diagnostics;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using RIS.Randomizing;

namespace Memenim.Pages
{
    public partial class ChatPlaceholderPage : PageContent
    {
        private static readonly FastSecureRandom RandomGenerator = new FastSecureRandom();
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
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            var biasZone =
                int.MaxValue - (int.MaxValue % Smiles.Length) - 1;
            int smileIndex =
                (int)RandomGenerator.GetUInt32((uint)biasZone) % Smiles.Length;

            if (Smiles[smileIndex] != txtSmile.Text)
            {
                txtSmile.Text = Smiles[smileIndex];
                return;
            }

            if (smileIndex == 0)
            {
                ++smileIndex;
            }
            else if (smileIndex == Smiles.Length - 1)
            {
                --smileIndex;
            }
            else
            {
                if (Rand.Current.NextBoolean(0.5))
                    ++smileIndex;
                else
                    --smileIndex;
            }

            txtSmile.Text = Smiles[smileIndex];
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

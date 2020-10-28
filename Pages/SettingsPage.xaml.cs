using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Data;
using Memenim.Localization;
using Memenim.Navigation;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class SettingsPage : PageContent
    {
        public Dictionary<string, string> Locales { get; } = new Dictionary<string, string>
        {
            {"en-US", "English" },
            {"ru-RU", "Русский" },
            {"ja-JP", "日本語" }
        };

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;

            slcLanguage.SelectedItem = new KeyValuePair<string, string>(
                SettingManager.AppSettings.Language,
                Locales[SettingManager.AppSettings.Language]);
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            wdgUserBanner.CurrentProfileData = new ProfileData
            {
                id = SettingManager.PersistentSettings.CurrentUserId
            };

            await wdgUserBanner.UpdateProfile()
                .ConfigureAwait(true);
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SettingManager.PersistentSettings.RemoveUser(
                SettingManager.PersistentSettings.CurrentUserLogin);

            NavigationController.Instance.RequestPage<LoginPage>();
        }

        private async void slcLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPair = (KeyValuePair<string, string>)slcLanguage.SelectedItem;

            await LocalizationManager.SwitchLanguage(selectedPair.Key)
                .ConfigureAwait(true);
        }
    }
}

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Memenim.Settings;
using Memenim.Managers;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : PageContent
    {
        public Dictionary<string, string> Locales { get; } = new Dictionary<string, string>()
        {
            {"en-US", "English" },
            {"ru-RU", "Русский" },
            {"ja-JP", "日本語" }
        };

        public SettingsPage() : base()
        {
            InitializeComponent();
            DataContext = this;

            slcLang.SelectedItem = new KeyValuePair<string, string>(
                SettingManager.AppSettings.Language,
                Locales[SettingManager.AppSettings.Language]);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EProfileData, AppPersistent.LocalUserId);
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            AppPersistent.RemoveFromStore("UserToken");
            AppPersistent.RemoveFromStore("UserId");

            PageNavigationManager.SwitchToPage<LoginPage>();
        }

        private void SplitButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPair = (KeyValuePair<string, string>)slcLang.SelectedItem;
            LocalizationManager.SwitchLanguage(selectedPair.Key);
        }
    }
}

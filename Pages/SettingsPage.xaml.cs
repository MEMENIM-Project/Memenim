using AnonymDesktopClient.Core;
using AnonymDesktopClient.Core.Pages;
using AnonymDesktopClient.Core.Utils;
using Memenim.Core;
using Memenim.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public Dictionary<string, string> Locales { get; } = new Dictionary<string, string>()
        {
            {"en-US", "English" },
            {"ru-RU", "Русский" },
            {"ja-JP", "日本語" }
        };

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EProfileData, AppPersistent.LocalUserId);
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            AppPersistent.RemoveFromStore("UserToken");
            AppPersistent.RemoveFromStore("UserId");

            PageNavigationManager.SwitchToPage(new LoginPage());
        }

        private void SplitButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPair = (KeyValuePair<string, string>)slcLang.SelectedItem;
            LocalizationManager.SwitchLanguage(LocalizationManager.MainWindow, selectedPair.Key);
        }
    }
}

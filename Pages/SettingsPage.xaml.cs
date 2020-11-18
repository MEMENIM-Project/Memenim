using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Schema;
using Memenim.Localization;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class SettingsPage : PageContent
    {
        public static Dictionary<string, string> Locales { get; } = new Dictionary<string, string>
        {
            {"en-US", "English" },
            {"ru-RU", "Русский" },
            {"ja-JP", "日本語" }
        };

        public SettingsViewModel ViewModel
        {
            get
            {
                return DataContext as SettingsViewModel;
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();

            slcLanguage.SelectedItem = new KeyValuePair<string, string>(
                SettingsManager.AppSettings.Language,
                Locales[SettingsManager.AppSettings.Language]);
        }

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        loadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    loadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            base.OnEnter(sender, e);

            wdgUserBanner.CurrentProfileData = new ProfileSchema
            {
                id = SettingsManager.PersistentSettings.CurrentUserId
            };

            await wdgUserBanner.UpdateProfile()
                .ConfigureAwait(true);

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.PersistentSettings.RemoveUser(
                SettingsManager.PersistentSettings.CurrentUserLogin);

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

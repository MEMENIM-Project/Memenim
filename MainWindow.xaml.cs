using System;
using System.Security.Cryptography;
using System.Windows;
using Memenim.Pages;
using Memenim.Settings;
using MahApps.Metro.Controls;
using Memenim.Localization;
using Memenim.Navigation;
using Memenim.Utils;
using RIS.Extensions;

namespace Memenim
{
    public sealed partial class MainWindow : MetroWindow
    {
        public static MainWindow _instance;
        public static MainWindow Instance
        {
            get
            {
                return _instance ??= new MainWindow();
            }
        }

        private MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _instance = this;

            rootLayout.Children.Add(NavigationController.Instance);

            Width = SettingsManager.AppSettings.WindowWidth;
            Height = SettingsManager.AppSettings.WindowHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SettingsManager.AppSettings.WindowPositionX - (Width / 2.0);
            Top = SettingsManager.AppSettings.WindowPositionY - (Height / 2.0);
            WindowState = (WindowState)SettingsManager.AppSettings.WindowState;

            LocalizationManager.SwitchLanguage(SettingsManager.AppSettings.Language).Wait();

            try
            {
                SettingsManager.PersistentSettings.CurrentUserLogin =
                    SettingsManager.PersistentSettings.GetCurrentUserLogin();

                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUserLogin))
                {
                    NavigationController.Instance.RequestPage<LoginPage>();
                    return;
                }

                string userToken = SettingsManager.PersistentSettings.GetUserToken(
                    SettingsManager.PersistentSettings.CurrentUserLogin);
                string userId = SettingsManager.PersistentSettings.GetUserId(
                    SettingsManager.PersistentSettings.CurrentUserLogin);

                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(userId))
                {
                    SettingsManager.PersistentSettings.CurrentUserToken =
                        PersistentUtils.WinUnprotect(userToken, $"UserToken-{SettingsManager.PersistentSettings.CurrentUserLogin}");
                    SettingsManager.PersistentSettings.CurrentUserId =
                        PersistentUtils.WinUnprotect(userId, $"UserId-{SettingsManager.PersistentSettings.CurrentUserLogin}").ToInt();

                    NavigationController.Instance.RequestPage<FeedPage>();
                }
                else
                {
                    NavigationController.Instance.RequestPage<LoginPage>();
                }
            }
            catch (CryptographicException)
            {
                NavigationController.Instance.RequestPage<LoginPage>();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SettingsManager.AppSettings.WindowWidth = Width;
            SettingsManager.AppSettings.WindowHeight = Height;
            SettingsManager.AppSettings.WindowPositionX = Left + (Width / 2.0);
            SettingsManager.AppSettings.WindowPositionY = Top + (Height / 2.0);
            SettingsManager.AppSettings.WindowState = (int)WindowState;

            SettingsManager.AppSettings.Save();
        }
    }
}

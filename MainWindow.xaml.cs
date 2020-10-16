using System;
using System.Security.Cryptography;
using System.Windows;
using AnonymDesktopClient.Core.Pages;
using AnonymDesktopClient.Core.Settings;
using AnonymDesktopClient.Managers;
using MahApps.Metro.Controls;
using RIS.Extensions;

namespace AnonymDesktopClient.Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow CurrentMainWindow;

        public MainWindow()
        {
            InitializeComponent();

            CurrentMainWindow = this;

            SettingManager.MainWindow = this;

            Width = SettingManager.AppSettings.WindowWidth;
            Height = SettingManager.AppSettings.WindowHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SettingManager.AppSettings.WindowPositionX - (Width / 2.0);
            Top = SettingManager.AppSettings.WindowPositionY - (Height / 2.0);
            WindowState = (WindowState)SettingManager.AppSettings.WindowState;

            PageNavigationManager.PageContentControl = contentArea;
            DialogManager.WindowRef = this;

            LocalizationManager.MainWindow = this;
            LocalizationManager.SwitchLanguage(SettingManager.AppSettings.Language);

            try
            {
                string userToken = AppPersistent.GetFromStore("UserToken");
                string userId = AppPersistent.GetFromStore("UserId");

                if (userToken != null && userId != null)
                {
                    AppPersistent.UserToken = AppPersistent.WinUnprotect(userToken, "UserToken");
                    AppPersistent.LocalUserId = AppPersistent.WinUnprotect(userId, "UserId").ToInt();

                    PageNavigationManager.SwitchToPage<ApplicationPage>();
                }
                else
                {
                    PageNavigationManager.SwitchToPage<LoginPage>();
                }
            }
            catch (CryptographicException)
            {
                PageNavigationManager.SwitchToPage<LoginPage>();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SettingManager.AppSettings.WindowWidth = Width;
            SettingManager.AppSettings.WindowHeight = Height;
            SettingManager.AppSettings.WindowPositionX = Left + (Width / 2.0);
            SettingManager.AppSettings.WindowPositionY = Top + (Height / 2.0);
            SettingManager.AppSettings.WindowState = (int)WindowState;
            SettingManager.AppSettings.Save();
        }
    }
}

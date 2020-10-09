using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using AnonymDesktopClient.Core;
using AnonymDesktopClient.Pages;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RIS.Extensions;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            PageNavigationManager.PageContentControl = contentArea;
            DialogManager.WindowRef = this;

            try
            {
                string userToken = AppPersistent.GetFromStore("UserToken");
                string userId = AppPersistent.GetFromStore("UserId");

                if (userToken != null && userId != null)
                {
                    AppPersistent.UserToken = AppPersistent.WinUnprotect(userToken, "UserToken");
                    AppPersistent.LocalUserId = AppPersistent.WinUnprotect(userId, "UserId").ToInt();

                    PageNavigationManager.SwitchToPage(new ApplicationPage());
                }
                else
                {
                    PageNavigationManager.SwitchToPage(new LoginPage());
                }
            }
            catch (CryptographicException)
            {
                PageNavigationManager.SwitchToPage(new LoginPage());
            }
        }
    }
}

using AnonymDesktopClient.Core;
using Memenim.Core;
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
    /// Interaction logic for RegisterUser.xaml
    /// </summary>
    public partial class RegisterUser : UserControl
    {
        public RegisterUser()
        {
            InitializeComponent();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            btnRegister.IsEnabled = false;

            try
            {
                var result = await UsersAPI.RegisterUser(txtLogin.Text, txtPass.Password);

                if(result.error)
                {
                    lblErrorMessage.Text = result.message;
                    btnRegister.IsEnabled = true;
                }
                else
                {
                    AppPersistent.UserToken = result.data.token;
                    AppPersistent.LocalUserId = result.data.id;
                    PageNavigationManager.SwitchToPage(new ApplicationPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnAutoReg_Click(object sender, RoutedEventArgs e)
        {
            UInt64 counter = 0;
            string name = "bot";
            var res = await UsersAPI.RegisterUser(name + counter.ToString("D10"), "botpass");
            while(res.error)
            {
                ++counter;
                res = await UsersAPI.RegisterUser(name + counter.ToString("D10"), "botpass");
            }
            AppPersistent.UserToken = res.data.token;
            AppPersistent.LocalUserId = res.data.id;
            DialogManager.ShowDialog("S U C C", "Regisered user with nickname " + name + counter.ToString("D10"));
            PageNavigationManager.SwitchToPage(new ApplicationPage());
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new LoginPage());
        }
    }
}

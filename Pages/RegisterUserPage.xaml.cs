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
                var res = await ApiHelper.RegisterUser(txtLogin.Text, txtPass.Password);
                if (res != null)
                {
                    lblErrorMessage.Text = res.message;
                    btnRegister.IsEnabled = true;
                }
                else
                {
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
            var res = await ApiHelper.RegisterUser(name + counter.ToString("D10"), "botpass");
            while(res.error)
            {
                ++counter;
                res = await ApiHelper.RegisterUser(name + counter.ToString("D10"), "botpass");
            }
            DialogManager.ShowDialog("S U C C", "Regisered user with nickname " + name + counter.ToString("D10"));
            PageNavigationManager.SwitchToPage(new ApplicationPage());
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.SwitchToPage(new LoginPage());
        }
    }
}

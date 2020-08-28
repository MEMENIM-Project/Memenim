using AnonymDesktopClient.DataStructs;
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
    /// Interaction logic for MemesPage.xaml
    /// </summary>
    public partial class MemesPage : UserControl
    {
        public MemesPage()
        {
            InitializeComponent();
        }

        private async void btnSteal_Click(object sender, RoutedEventArgs e)
        {
            UserInfo victimData = await ApiHelper.GetUserInfo(System.Convert.ToInt32(txtStealId.Text));

            await ApiHelper.EditUserInfo(victimData);
            MessageBox.Show("Done. Kinda");
        }
    }
}

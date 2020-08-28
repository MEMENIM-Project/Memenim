using AnonymDesktopClient.DataStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        void LockButtons(bool state = true)
        {
            btnSharesBoost.IsEnabled = !state;
            btnViewsBoost.IsEnabled = !state;
        }

        // Not the best solution to do this(The spam stops after ~400 iterations)
        private async void btnSharesBoost_Click(object sender, RoutedEventArgs e)
        {
            LockButtons(true);
            try
            {
                for (int i = 0; i < Convert.ToInt32(txtSharesCount.Text); ++i)
                {
                    await ApiHelper.AddShares(Convert.ToInt32(txtGroupId.Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            LockButtons(false);
            MessageBox.Show("Done");
        }

        private async void btnViewsBoost_Click(object sender, RoutedEventArgs e)
        {
            LockButtons(true);
            try
            {
                for (int i = 0; i < Convert.ToInt32(txtViewsCount.Text); ++i)
                {
                    await ApiHelper.AddView(Convert.ToInt32(txtGroupId.Text));
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("Done");
            LockButtons(false);

        }
    }
}

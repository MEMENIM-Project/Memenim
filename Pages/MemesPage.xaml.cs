using AnonymDesktopClient.DataStructs;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Converters;
using Microsoft.Win32;
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
using System.IO;

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for MemesPage.xaml
    /// </summary>
    public partial class MemesPage : UserControl
    {

        private string[] m_SpamCommentsList;

        public MemesPage()
        {
            InitializeComponent();
        }

        private async void btnSteal_Click(object sender, RoutedEventArgs e)
        {
            ProfileData victimData = await ApiHelper.GetUserInfo(System.Convert.ToInt32(txtStealId.Value));

            await ApiHelper.EditUserInfo(victimData);
            DialogManager.ShowDialog("Success", "Profile copied");
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
                for (int i = 0; i < Convert.ToInt32(txtSharesCount.Value); ++i)
                {
                    await ApiHelper.AddShares(Convert.ToInt32(txtGroupId.Value));
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }
            LockButtons(false);
            DialogManager.ShowDialog("Success", "BOOOOSTED");
        }

        private async void btnViewsBoost_Click(object sender, RoutedEventArgs e)
        {
            LockButtons(true);
            try
            {
                for (int i = 0; i < Convert.ToInt32(txtViewsCount.Value); ++i)
                {
                    await ApiHelper.AddView(Convert.ToInt32(txtGroupId.Value));
                }

            }
            catch(Exception ex)
            {
                DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }
            DialogManager.ShowDialog("Success", "BOOOOSTED");
            LockButtons(false);

        }

        private async void btnSpamComments_Click(object sender, RoutedEventArgs e)
        {
            if (m_SpamCommentsList != null && m_SpamCommentsList.Length > 0)
            {
                btnSpamComments.IsEnabled = false;
                try
                {
                    Random rnd = new Random();
                    for (int i = 0; i < txtCommentsCount.Value; ++i)
                    {
                        int idx = rnd.Next(0, m_SpamCommentsList.Length - 1);
                        await ApiHelper.SendComment((int)txtPostId.Value, m_SpamCommentsList[idx], bAnonymousComments.IsChecked);
                    }
                }
                catch (Exception ex)
                {
                    DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
                }
                DialogManager.ShowDialog("Success", "Comment section destoroyed ;)");
                btnSpamComments.IsEnabled = true;
            }
        }

        private void btnOpenCommentsFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            try
            {
                if (dlg.ShowDialog().Value)
                {
                    txtCommentsFilePath.Text = dlg.FileName;
                    m_SpamCommentsList = File.ReadAllLines(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }
        }
    }
}

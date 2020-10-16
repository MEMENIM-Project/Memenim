using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Microsoft.Win32;

namespace Memenim.Pages
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
            var victimData = await UserApi.GetProfileById(System.Convert.ToInt32(txtStealId.Value));

            await UserApi.EditProfile(victimData.data[0], AppPersistent.UserToken);
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
                    await PostApi.AddRepost(Convert.ToInt32(txtGroupId.Value), AppPersistent.UserToken);
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
                    await PostApi.AddView(Convert.ToInt32(txtGroupId.Value), AppPersistent.UserToken);
                }

            }
            catch (Exception ex)
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
                        var res = await PostApi.AddComment((int)txtPostId.Value, m_SpamCommentsList[idx], bAnonymousComments.IsChecked, AppPersistent.UserToken);
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

        private async void btnEditPost_Click(object sender, RoutedEventArgs e)
        {
            EditPostRequest postRequest = new EditPostRequest();
            postRequest.id = (int)txtGroupId.Value;
            postRequest.text = txtPostText.Text;
            try
            {
                var res = await PostApi.EditPost(postRequest, AppPersistent.UserToken);
                if (!res.error)
                {
                    DialogManager.ShowDialog("S U C C", "Post editing done");
                }
                else
                {
                    DialogManager.ShowDialog("Not S U C C", res.message);
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }
        }
    }
}

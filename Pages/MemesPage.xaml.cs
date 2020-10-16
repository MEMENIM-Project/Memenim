using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core;
using Memenim.Core.Data;
using Microsoft.Win32;

namespace AnonymDesktopClient.Core.Pages
{
    /// <summary>
    /// Interaction logic for MemesPage.xaml
    /// </summary>
    public partial class MemesPage : Page
    {

        private string[] m_SpamCommentsList;

        public MemesPage() : base()
        {
            InitializeComponent();
        }

        private async void btnSteal_Click(object sender, RoutedEventArgs e)
        {
            var victimData = await UsersAPI.GetUserProfileByID(System.Convert.ToInt32(txtStealId.Value));

            await UsersAPI.EditProfile(victimData.data[0], AppPersistent.UserToken);
            await DialogManager.ShowDialog("Success", "Profile copied");
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
                    await PostAPI.AddRepost(Convert.ToInt32(txtGroupId.Value), AppPersistent.UserToken);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }
            LockButtons(false);
            await DialogManager.ShowDialog("Success", "BOOOOSTED");
        }

        private async void btnViewsBoost_Click(object sender, RoutedEventArgs e)
        {
            LockButtons(true);
            try
            {
                for (int i = 0; i < Convert.ToInt32(txtViewsCount.Value); ++i)
                {
                    await PostAPI.AddView(Convert.ToInt32(txtGroupId.Value), AppPersistent.UserToken);
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
                        var res = await PostAPI.SendComment((int)txtPostId.Value, m_SpamCommentsList[idx], bAnonymousComments.IsChecked, AppPersistent.UserToken);
                    }
                }
                catch (Exception ex)
                {
                    await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
                }
                await DialogManager.ShowDialog("Success", "Comment section destoroyed ;)");
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
            EditPostData postData = new EditPostData();
            postData.id = (int)txtGroupId.Value;
            postData.text = txtPostText.Text;
            try
            {
                var res = await PostAPI.EditPost(postData, AppPersistent.UserToken);
                if (!res.error)
                {
                    await DialogManager.ShowDialog("S U C C", "Post editing done");
                }
                else
                {
                    await DialogManager.ShowDialog("Not S U C C", res.message);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }
        }
    }
}

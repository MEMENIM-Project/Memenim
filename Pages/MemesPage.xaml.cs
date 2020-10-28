using System;
using System.IO;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Dialogs;
using Memenim.Settings;
using Microsoft.Win32;

namespace Memenim.Pages
{
    public partial class MemesPage : PageContent
    {
        private static readonly Random Random = new Random();

        private string[] _spamCommentsList;

        public MemesPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void btnSteal_Click(object sender, RoutedEventArgs e)
        {
            btnSteal.IsEnabled = false;

            try
            {
                var victimData = await UserApi.GetProfileById(Convert.ToInt32(txtStealId.Value))
                    .ConfigureAwait(true);

                await UserApi.EditProfile(victimData.data[0], SettingManager.PersistentSettings.CurrentUserToken)
                    .ConfigureAwait(true);

                await DialogManager.ShowDialog("Success", "Profile copied")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSteal.IsEnabled = true;
            }
        }

        private async void btnSpamComments_Click(object sender, RoutedEventArgs e)
        {
            if (_spamCommentsList == null || _spamCommentsList.Length == 0)
                return;

            btnSpamComments.IsEnabled = false;

            try
            {
                for (int i = 0; i < txtCommentsCount.Value; ++i)
                {
                    await PostApi.AddComment(int.Parse(txtCommentsPostId?.Value?.ToString() ?? string.Empty),
                            _spamCommentsList[Random.Next(0, _spamCommentsList.Length - 1)],
                            chkAnonymousComments.IsChecked, SettingManager.PersistentSettings.CurrentUserToken)
                        .ConfigureAwait(true);
                }

                await DialogManager.ShowDialog("Success", "Comment section destroyed ;)")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSpamComments.IsEnabled = true;
            }
        }

        private async void btnOpenCommentsFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            try
            {
                if (fileDialog?.ShowDialog() != true)
                    return;

                txtCommentsFilePath.Text = fileDialog.FileName;
                _spamCommentsList = await File.ReadAllLinesAsync(fileDialog.FileName)
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
        }

        private async void btnSharesBoost_Click(object sender, RoutedEventArgs e)
        {
            btnSharesBoost.IsEnabled = false;

            try
            {
                for (int i = 0; i < Convert.ToInt32(txtSharesCount.Value); ++i)
                {
                    await PostApi.AddRepost(Convert.ToInt32(txtPostsPostId.Value), SettingManager.PersistentSettings.CurrentUserToken)
                        .ConfigureAwait(true);
                }

                await DialogManager.ShowDialog("Success", "BOOOOSTED")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSharesBoost.IsEnabled = true;
            }
        }

        private async void btnViewsBoost_Click(object sender, RoutedEventArgs e)
        {
            btnViewsBoost.IsEnabled = false;

            try
            {
                for (int i = 0; i < Convert.ToInt32(txtViewsCount.Value); ++i)
                {
                    await PostApi.AddView(Convert.ToInt32(txtPostsPostId.Value), SettingManager.PersistentSettings.CurrentUserToken)
                        .ConfigureAwait(true);
                }

                await DialogManager.ShowDialog("Success", "BOOOOSTED")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnViewsBoost.IsEnabled = true;
            }
        }

        private async void btnEditPost_Click(object sender, RoutedEventArgs e)
        {
            btnEditPost.IsEnabled = false;

            EditPostRequest postRequest = new EditPostRequest
            {
                id = Convert.ToInt32(txtPostsPostId.Value),
                text = txtPostText.Text
            };

            try
            {
                var result = await PostApi.EditPost(postRequest, SettingManager.PersistentSettings.CurrentUserToken)
                    .ConfigureAwait(true);

                if (!result.error)
                {
                    await DialogManager.ShowDialog("S U C C", "Post editing done")
                        .ConfigureAwait(true);
                }
                else
                {
                    await DialogManager.ShowDialog("Not S U C C", result.message)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnEditPost.IsEnabled = true;
            }
        }
    }
}

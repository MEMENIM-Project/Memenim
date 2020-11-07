using System;
using System.IO;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Schema;
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

                if (victimData.data == null)
                    throw new Exception();

                await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken, victimData.data)
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
                    await PostApi.AddComment(SettingsManager.PersistentSettings.CurrentUserToken,
                            int.Parse(txtCommentsPostId?.Value?.ToString() ?? string.Empty),
                            _spamCommentsList[Random.Next(0, _spamCommentsList.Length - 1)],
                            chkAnonymousComments.IsChecked)
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
                    await PostApi.AddRepost(Convert.ToInt32(txtPostsPostId.Value))
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
                    await PostApi.AddView(Convert.ToInt32(txtPostsPostId.Value))
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

            try
            {
                var result = await PostApi.GetById(SettingsManager.PersistentSettings.CurrentUserToken,
                        Convert.ToInt32(txtPostsPostId.Value))
                    .ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                    return;
                }

                if (result.data == null)
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                    return;
                }

                PostEditSchema postRequest = new PostEditSchema
                {
                    id = Convert.ToInt32(txtPostsPostId.Value),
                    text = txtPostText.Text,
                    adult = result.data.adult,
                    author_watch = result.data.author_watch,
                    category = result.data.category,
                    filter = result.data.filter,
                    hidden = result.data.hidden,
                    open_comments = result.data.open_comments,
                    type = result.data.type
                };

                var resultEdit = await PostApi.EditPost(SettingsManager.PersistentSettings.CurrentUserToken,
                        postRequest)
                    .ConfigureAwait(true);

                if (!resultEdit.error)
                {
                    await DialogManager.ShowDialog("S U C C", "Post editing done")
                        .ConfigureAwait(true);
                }
                else
                {
                    await DialogManager.ShowDialog("F U C K", resultEdit.message)
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

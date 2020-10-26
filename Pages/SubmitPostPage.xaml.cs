using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Memenim.Core.Api;
using Memenim.Core.Data;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class SubmitPostPage : UserControl
    {
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register("CurrentPostData", typeof(PostData), typeof(SubmitPostPage),
                new PropertyMetadata(new PostData
                {
                    owner_id = SettingManager.PersistentSettings.CurrentUserId,
                    open_comments = 1,
                    attachments = new List<AttachmentData>
                    {
                        new AttachmentData
                        {
                            photo = new PhotoData
                            {
                                photo_big = string.Empty,
                                photo_medium = string.Empty,
                                photo_small = string.Empty,
                                size = new PhotoSizeData()
                                {
                                    photo_big = new RectangleData(),
                                    photo_medium = new RectangleData(),
                                    photo_small = new RectangleData()
                                }
                            }
                        }
                    }
                }));

        public PostData CurrentPostData
        {
            get
            {
                return (PostData)GetValue(CurrentPostDataProperty);
            }
            set
            {
                SetValue(CurrentPostDataProperty, value);
            }
        }

        public SubmitPostPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void ShowPreviewImage()
        {
            imgPreview.Source = CurrentPostData?.attachments?[0]?.photo?.photo_medium != null
                                && Uri.TryCreate(CurrentPostData.attachments[0].photo.photo_medium, UriKind.Absolute, out Uri uri)
                ? new BitmapImage(uri)
                : null;
        }

        public void HidePreviewImage()
        {
            imgPreview.Source = null;
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            CurrentPostData.attachments[0].photo.photo_medium = url;

            ShowPreviewImage();
            wdgPostPreview.ShowImage();
        }

        public void ClearImage()
        {
            CurrentPostData.attachments[0].photo.photo_medium = string.Empty;

            HidePreviewImage();
            wdgPostPreview.HideImage();
        }

        private async Task SelectPhotoForPost(string url)
        {
            LoadImage(url);
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentPostData?.owner_id.HasValue == true)
            {
                var result = await UserApi.GetProfileById(CurrentPostData.owner_id.Value)
                    .ConfigureAwait(true);

                CurrentPostData.owner_name = result.data[0].name;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentPostData.author_watch++;

                var result = await PostApi.AddPost(CurrentPostData, SettingManager.PersistentSettings.CurrentUserToken)
                    .ConfigureAwait(true);

                if (!result.error)
                {
                    await DialogManager.ShowDialog("S U C C", "Post submitted. Get a tea and wait")
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
        }

        private async void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);

            if (rbImageRaw.IsChecked == true)
            {
                string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                    .ConfigureAwait(true);

                await SelectPhotoForPost(url)
                    .ConfigureAwait(true);
            }
            else if (rbImageTennor.IsChecked == true)
            {
                PageNavigationManager.SwitchToSubPage(new TennorSearchPage
                {
                    OnPicSelect = SelectPhotoForPost
                });
            }
            else if (rbImageGallery.IsChecked == true)
            {
                PageNavigationManager.SwitchToSubPage(new AnonymGallerySearchPage
                {
                    OnPicSelect = SelectPhotoForPost
                });
            }
        }

        private void ClearPhoto_Click(object sender, RoutedEventArgs e)
        {
            ClearImage();
        }
    }
}

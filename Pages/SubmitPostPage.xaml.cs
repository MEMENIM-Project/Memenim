using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AnonymDesktopClient.Managers;
using Memenim.Core;
using Memenim.Core.Data;

namespace AnonymDesktopClient.Core.Pages
{
    /// <summary>
    /// Interaction logic for SubmitPostPage.xaml
    /// </summary>
    public partial class SubmitPostPage : Page
    {
        private PostData m_PostData;

        public SubmitPostPage() : base()
        {
            InitializeComponent();
            m_PostData = new PostData();
            m_PostData.attachments = new List<AttachmentData>();
            m_PostData.attachments.Add(new AttachmentData());
            m_PostData.attachments[0].photo = new PhotoData()
            {
                photo_big = "",
                photo_medium = "",
                photo_small = "",
                size = new SizeData()
                {
                    photo_big = new RectData(),
                    photo_medium = new RectData(),
                    photo_small = new RectData()
                }
            };
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            DataContext = m_PostData;
            wdgPostPreview.CurrentPostData = m_PostData;
            wdgPostPreview.PreviewMode = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_PostData.author_watch++;
                var res = await PostAPI.SubmitPost(m_PostData, AppPersistent.UserToken);
                if (!res.error)
                {
                    await DialogManager.ShowDialog("S U C C", "Post submitted. Get a tea and wait");
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }

        }

        private async void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, this);
            if (rbImageRaw.IsChecked == true)
            {
                string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL");
                SelectPhotoForPost(url);
            }
            else if (rbImageTennor.IsChecked == true)
            {
                //PageNavigationManager.SwitchToSubpage(new TennorSearchPage() { OnPicSelect = SelectPhotoForPost });
            }
            else
            {
                //PageNavigationManager.SwitchToSubpage(new AnonymGallerySearchPage() { OnPicSelect = SelectPhotoForPost });
            }
        }

        private async Task SelectPhotoForPost(string url)
        {
            m_PostData.attachments[0].photo.photo_medium = url;
            imgPreview.Source = new BitmapImage(new Uri(m_PostData.attachments[0].photo.photo_medium));
            wdgPostPreview.ReloadImage();
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class SubmitPostPage : PageContent
    {
        public SubmitPostViewModel ViewModel
        {
            get
            {
                return DataContext as SubmitPostViewModel;
            }
        }

        public SubmitPostPage()
        {
            InitializeComponent();
            DataContext = new SubmitPostViewModel();
        }

        private Task SelectPhoto(string url)
        {
            LoadImage(url);

            return Task.CompletedTask;
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            ViewModel.CurrentPostData.attachments[0].photo.photo_medium = url;
        }

        public void ClearImage()
        {
            ViewModel.CurrentPostData.attachments[0].photo.photo_medium = string.Empty;
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            if (ViewModel.CurrentPostData?.owner_id.HasValue == true)
            {
                if (ViewModel.CurrentPostData.owner_id == -1)
                    return;

                var result = await UserApi.GetProfileById(ViewModel.CurrentPostData.owner_id.Value)
                    .ConfigureAwait(true);

                if (result.data == null)
                    return;

                ViewModel.CurrentPostData.owner_name = result.data.name;
                ViewModel.CurrentPostData.owner_photo = result.data.photo;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.CurrentPostData.author_watch++;

                var result = await PostApi.AddPost(SettingsManager.PersistentSettings.CurrentUserToken, ViewModel.CurrentPostData)
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
            if (rbImageRaw.IsChecked == true)
            {
                string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                    .ConfigureAwait(true);

                await SelectPhoto(url)
                    .ConfigureAwait(true);
            }
            else if (rbImageTenor.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
                {
                    OnPicSelect = SelectPhoto
                });
            }
            else if (rbImageGallery.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
                {
                    OnPicSelect = SelectPhoto
                });
            }
        }

        private void ClearPhoto_Click(object sender, RoutedEventArgs e)
        {
            ClearImage();
        }
    }
}

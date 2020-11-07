using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Settings;

namespace Memenim.Pages
{
    public partial class SubmitPostPage : PageContent
    {
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register(nameof(CurrentPostData), typeof(PostSchema), typeof(SubmitPostPage),
                new PropertyMetadata(new PostSchema
                {
                    owner_id = SettingsManager.PersistentSettings.CurrentUserId
                }));

        public PostSchema CurrentPostData
        {
            get
            {
                return (PostSchema)GetValue(CurrentPostDataProperty);
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

        private Task SelectPhoto(string url)
        {
            LoadImage(url);

            return Task.CompletedTask;
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            CurrentPostData.attachments[0].photo.photo_medium = url;
        }

        public void ClearImage()
        {
            CurrentPostData.attachments[0].photo.photo_medium = string.Empty;
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (CurrentPostData?.owner_id.HasValue == true)
            {
                if (CurrentPostData.owner_id == -1)
                    return;

                var result = await UserApi.GetProfileById(CurrentPostData.owner_id.Value)
                    .ConfigureAwait(true);

                if (result.data == null)
                    return;

                CurrentPostData.owner_name = result.data.name;
                CurrentPostData.owner_photo = result.data.photo;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CurrentPostData.author_watch++;

                var result = await PostApi.AddPost(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData)
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
            else if (rbImageTennor.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<TennorSearchPage>(new TennorSearchPage
                {
                    OnPicSelect = SelectPhoto
                });
            }
            else if (rbImageGallery.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchPage
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

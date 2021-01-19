using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Extensions;
using Memenim.Localization;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;

namespace Memenim.Pages
{
    public partial class SubmitPostPage : PageContent
    {
        public ReadOnlyDictionary<int, string> PostCategories { get; private set; }

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

            ReloadPostCategories();

            slcPostCategories.SelectedIndex = 0;

            LocalizationManager.LanguageChanged += OnLanguageChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
            ProfileUtils.AvatarChanged += OnAvatarChanged;
            ProfileUtils.NameChanged += OnNameChanged;
        }

        ~SubmitPostPage()
        {
            LocalizationManager.LanguageChanged -= OnLanguageChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
            ProfileUtils.AvatarChanged -= OnAvatarChanged;
            ProfileUtils.NameChanged -= OnNameChanged;
        }

        private void ReloadPostCategories()
        {
            var categories = PostApi.PostCategories.Values.ToArray();
            var localizedNames = PostCategorySchemaExtensions.GetLocalizedNames();
            var postCategories = new Dictionary<int, string>(categories.Length);

            for (var i = 0; i < categories.Length; ++i)
            {
                postCategories.Add(
                    categories[i].id,
                    localizedNames[i]);
            }

            slcPostCategories.SelectionChanged -= slcPostCategories_SelectionChanged;

            var selectedIndex = slcPostCategories.SelectedIndex;

            PostCategories = new ReadOnlyDictionary<int, string>(postCategories);

            slcPostCategories
                .GetBindingExpression(ItemsControl.ItemsSourceProperty)?
                .UpdateTarget();

            slcPostCategories.SelectedIndex = selectedIndex;

            slcPostCategories.SelectionChanged += slcPostCategories_SelectionChanged;
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

        public void ClearText()
        {
            ViewModel.CurrentPostData.text = string.Empty;
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

                var result = await UserApi.GetProfileById(
                        ViewModel.CurrentPostData.owner_id.Value)
                    .ConfigureAwait(true);

                if (result.data == null)
                    return;

                ViewModel.CurrentPostData.owner_name = result.data.name;
                ViewModel.CurrentPostData.owner_photo = result.data.photo;
            }
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            base.OnExit(sender, e);
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ReloadPostCategories();
        }

        private async void OnCurrentUserChanged(object sender, UserChangedEventArgs e)
        {
            if (e.NewUser.Id == -1)
                return;

            if (ViewModel.CurrentPostData == null)
                return;

            ViewModel.CurrentPostData.owner_id = e.NewUser.Id;

            if (ViewModel.CurrentPostData.owner_id == -1)
                return;

            var result = await UserApi.GetProfileById(
                    ViewModel.CurrentPostData.owner_id.Value)
                .ConfigureAwait(true);

            if (result.data == null)
                return;

            ViewModel.CurrentPostData.owner_name = result.data.name;
            ViewModel.CurrentPostData.owner_photo = result.data.photo;
        }

        private void OnAvatarChanged(object sender, UserPhotoChangedEventArgs e)
        {
            ViewModel.CurrentPostData.owner_photo = e.NewPhoto;
        }

        private void OnNameChanged(object sender, UserNameChangedEventArgs e)
        {
            ViewModel.CurrentPostData.owner_name = e.NewName;
        }

        private void slcPostCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentPostData.category = ((KeyValuePair<int, string>)slcPostCategories.SelectedItem).Key;
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

        private void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            ClearImage();
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            btnSubmit.IsEnabled = false;

            try
            {
                ViewModel.CurrentPostData.author_watch++;

                var result = await PostApi.Add(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        ViewModel.CurrentPostData)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowErrorDialog(result.message)
                        .ConfigureAwait(true);
                }
                else
                {
                    await DialogManager.ShowSuccessDialog("Post submitted. Get a tea and wait")
                        .ConfigureAwait(true);

                    ClearText();
                    ClearImage();
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSubmit.IsEnabled = true;
            }
        }
    }
}

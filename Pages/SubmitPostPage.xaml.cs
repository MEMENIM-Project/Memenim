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
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using RIS.Localization;

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

            LocalizationUtils.LocalizationChanged += OnLocalizationChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
            ProfileUtils.AvatarChanged += OnAvatarChanged;
            ProfileUtils.NameChanged += OnNameChanged;
        }

        ~SubmitPostPage()
        {
            LocalizationUtils.LocalizationChanged -= OnLocalizationChanged;
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
                    categories[i].Id,
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

        private Task SelectImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return Task.CompletedTask;

            ViewModel.CurrentPostData.Attachments[0].Photo.MediumUrl = url;

            return Task.CompletedTask;
        }

        private void ClearImage()
        {
            ViewModel.CurrentPostData.Attachments[0].Photo.MediumUrl = string.Empty;
        }

        private void ClearText()
        {
            ViewModel.CurrentPostData.Text = string.Empty;
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            if (ViewModel.CurrentPostData?.OwnerId.HasValue == true)
            {
                if (ViewModel.CurrentPostData.OwnerId == -1)
                    return;

                var result = await UserApi.GetProfileById(
                        ViewModel.CurrentPostData.OwnerId.Value)
                    .ConfigureAwait(true);

                if (result.Data == null)
                    return;

                ViewModel.CurrentPostData.OwnerNickname = result.Data.Nickname;
                ViewModel.CurrentPostData.OwnerPhotoUrl = result.Data.PhotoUrl;
            }
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        private void OnLocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            ReloadPostCategories();
        }

        private async void OnCurrentUserChanged(object sender, UserChangedEventArgs e)
        {
            if (e.NewUser.Id == -1)
                return;

            if (ViewModel.CurrentPostData == null)
                return;

            ViewModel.CurrentPostData.OwnerId = e.NewUser.Id;

            if (ViewModel.CurrentPostData.OwnerId == -1)
                return;

            var result = await UserApi.GetProfileById(
                    ViewModel.CurrentPostData.OwnerId.Value)
                .ConfigureAwait(true);

            if (result.Data == null)
                return;

            ViewModel.CurrentPostData.OwnerNickname = result.Data.Nickname;
            ViewModel.CurrentPostData.OwnerPhotoUrl = result.Data.PhotoUrl;
        }

        private void OnAvatarChanged(object sender, UserPhotoChangedEventArgs e)
        {
            ViewModel.CurrentPostData.OwnerPhotoUrl = e.NewPhoto;
        }

        private void OnNameChanged(object sender, UserNameChangedEventArgs e)
        {
            ViewModel.CurrentPostData.OwnerNickname = e.NewName;
        }

        private void slcPostCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.CurrentPostData.CategoryId = ((KeyValuePair<int, string>)slcPostCategories.SelectedItem).Key;
        }

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            if (rbImageRaw.IsChecked == true)
            {
                string title = LocalizationUtils.GetLocalized("InsertingImageTitle");
                string message = LocalizationUtils.GetLocalized("EnterURL");

                string url = await DialogManager.ShowSinglelineTextDialog(
                        title, message)
                    .ConfigureAwait(true);

                await SelectImage(url)
                    .ConfigureAwait(true);
            }
            else if (rbImageGallery.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
                {
                    ImageSelectionDelegate = SelectImage
                });
            }
            else if (rbImageTenor.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
                {
                    ImageSelectionDelegate = SelectImage
                });
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            ClearImage();
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            btnSubmit.IsEnabled = false;

            btnSubmit.Focus();

            try
            {
                var result = await PostApi.Add(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        ViewModel.CurrentPostData)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);
                }
                else
                {
                    var message = LocalizationUtils.GetLocalized("PostSubmittedMessage");

                    await DialogManager.ShowSuccessDialog(message)
                        .ConfigureAwait(true);

                    ClearText();
                    ClearImage();
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSubmit.IsEnabled = true;
            }
        }
    }
}

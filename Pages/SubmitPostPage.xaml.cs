using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            PostCategoriesComboBox.SelectedIndex = 0;

            LocalizationUtils.LocalizationUpdated += OnLocalizationUpdated;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
            ProfileUtils.AvatarChanged += OnAvatarChanged;
            ProfileUtils.NameChanged += OnNameChanged;
        }

        ~SubmitPostPage()
        {
            LocalizationUtils.LocalizationUpdated -= OnLocalizationUpdated;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
            ProfileUtils.AvatarChanged -= OnAvatarChanged;
            ProfileUtils.NameChanged -= OnNameChanged;
        }



        private void ReloadPostCategories()
        {
            var localizedNames = PostCategorySchemaExtensions
                .GetLocalizedNames();
            var postCategories = new Dictionary<int, string>(
                PostCategorySchemaExtensions.Categories.Length);

            for (var i = 0; i < PostCategorySchemaExtensions.Categories.Length; ++i)
            {
                postCategories.Add(
                    PostCategorySchemaExtensions
                        .Categories[i].Id,
                    localizedNames[i]);
            }

            PostCategoriesComboBox.SelectionChanged -= PostCategoriesComboBox_SelectionChanged;

            var selectedIndex = PostCategoriesComboBox.SelectedIndex;

            PostCategories = new ReadOnlyDictionary<int, string>(
                postCategories);

            PostCategoriesComboBox
                .GetBindingExpression(ItemsControl.ItemsSourceProperty)?
                .UpdateTarget();

            PostCategoriesComboBox.SelectedIndex = selectedIndex;

            PostCategoriesComboBox.SelectionChanged += PostCategoriesComboBox_SelectionChanged;
        }

        private Task SelectImage(
            string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return Task.CompletedTask;

            ViewModel.CurrentPostData.Attachments[0].Photo.MediumUrl = url;

            return Task.CompletedTask;
        }

        private void ClearImage()
        {
            ViewModel.CurrentPostData
                .Attachments[0].Photo.MediumUrl = string.Empty;
        }

        private void ClearText()
        {
            ViewModel.CurrentPostData
                .Text = string.Empty;
        }



        protected override async void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            if (ViewModel.CurrentPostData == null
                || !ViewModel.CurrentPostData.OwnerId.HasValue
                || ViewModel.CurrentPostData.OwnerId == -1)
            {
                return;
            }

            var result = await UserApi.GetProfileById(
                    ViewModel.CurrentPostData.OwnerId.Value)
                .ConfigureAwait(true);

            if (result.Data == null)
                return;

            ViewModel.CurrentPostData.OwnerNickname =
                result.Data.Nickname;
            ViewModel.CurrentPostData.OwnerPhotoUrl =
                result.Data.PhotoUrl;
        }

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private void OnLocalizationUpdated(object sender,
            LocalizationEventArgs e)
        {
            ReloadPostCategories();
        }

        private async void OnCurrentUserChanged(object sender,
            UserChangedEventArgs e)
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

            ViewModel.CurrentPostData.OwnerNickname =
                result.Data.Nickname;
            ViewModel.CurrentPostData.OwnerPhotoUrl =
                result.Data.PhotoUrl;
        }

        private void OnAvatarChanged(object sender,
            UserPhotoChangedEventArgs e)
        {
            ViewModel.CurrentPostData.OwnerPhotoUrl =
                e.NewPhoto;
        }

        private void OnNameChanged(object sender,
            UserNameChangedEventArgs e)
        {
            ViewModel.CurrentPostData.OwnerNickname =
                e.NewName;
        }



        private void PostCategoriesComboBox_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            ViewModel.CurrentPostData.CategoryId =
                ((KeyValuePair<int, string>)PostCategoriesComboBox.SelectedItem)
                .Key;
        }

        private async void SelectImageButton_Click(object sender,
            RoutedEventArgs e)
        {
            if (LoadImageFromUrlRadioButton.IsChecked == true)
            {
                var title = LocalizationUtils
                    .GetLocalized("InsertingImageTitle");
                var message = LocalizationUtils
                    .GetLocalized("EnterUrl");

                var url = await DialogManager.ShowSinglelineTextDialog(
                        title, message)
                    .ConfigureAwait(true);

                await SelectImage(url)
                    .ConfigureAwait(true);
            }
            else if (LoadImageFromGalleryRadioButton.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
                {
                    ImageSelectionDelegate = SelectImage
                });
            }
            else if (LoadImageFromTenorRadioButton.IsChecked == true)
            {
                NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
                {
                    ImageSelectionDelegate = SelectImage
                });
            }
        }

        private void RemoveImageButton_Click(object sender,
            RoutedEventArgs e)
        {
            ClearImage();
        }

        private async void SubmitButton_Click(object sender,
            RoutedEventArgs e)
        {
            SubmitButton.IsEnabled = false;

            SubmitButton.Focus();

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
                    var message = LocalizationUtils
                        .GetLocalized("PostSubmittedMessage");

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
                SubmitButton.IsEnabled = true;
            }
        }
    }
}

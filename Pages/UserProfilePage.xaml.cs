using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Extensions;
using Memenim.Localization;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
using WpfAnimatedGif;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class UserProfilePage : PageContent
    {
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register(nameof(IsEditMode), typeof(bool), typeof(UserProfilePage),
                new PropertyMetadata(false));

        private readonly SemaphoreSlim _profileUpdateLock = new SemaphoreSlim(1, 1);
        private int _profileUpdateWaitingCount;
        private bool _loadingGridShowing;

        public UserProfileViewModel ViewModel
        {
            get
            {
                return DataContext as UserProfileViewModel;
            }
        }

        public bool IsEditMode
        {
            get
            {
                return (bool)GetValue(IsEditModeProperty);
            }
            set
            {
                SetValue(IsEditModeProperty, value);
            }
        }

        public UserProfilePage()
        {
            InitializeComponent();
            DataContext = new UserProfileViewModel();

            LocalizationManager.LanguageChanged += OnLanguageChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
        }

        ~UserProfilePage()
        {
            LocalizationManager.LanguageChanged -= OnLanguageChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
        }

        public Task UpdateProfile()
        {
            return UpdateProfile(ViewModel.CurrentProfileData.Id);
        }
        public async Task UpdateProfile(int id)
        {
            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            try
            {
                Interlocked.Increment(ref _profileUpdateWaitingCount);

                await _profileUpdateLock.WaitAsync()
                    .ConfigureAwait(true);
            }
            finally
            {
                Interlocked.Decrement(ref _profileUpdateWaitingCount);
            }

            try
            {
                btnEditMode.IsChecked = false;

                if (id < 0)
                {
                    if (!NavigationController.Instance.IsCurrentPage<UserProfilePage>())
                        return;

                    NavigationController.Instance.GoBack(true);

                    string message = LocalizationUtils.GetLocalized("UserNotFound");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                var result = await UserApi.GetProfileById(id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    if (!NavigationController.Instance.IsCurrentPage<UserProfilePage>())
                        return;

                    NavigationController.Instance.GoBack(true);

                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (result.Data == null)
                {
                    if (!NavigationController.Instance.IsCurrentPage<UserProfilePage>())
                        return;

                    NavigationController.Instance.GoBack(true);

                    string message = LocalizationUtils.GetLocalized("UserNotFound");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                ViewModel.CurrentProfileData = result.Data;

                UpdateStatBlock(wpStatBlock1, wpStatBlock2,
                    wpStatBlock3, wpStatBlock4);
            }
            finally
            {
                await Task.Delay(500)
                    .ConfigureAwait(true);

                if (_profileUpdateWaitingCount == 0)
                {
                    await ShowLoadingGrid(false)
                        .ConfigureAwait(true);
                }

                _profileUpdateLock.Release();
            }
        }

        public void UpdateStatBlock(params StackPanel[] statBlocks)
        {
            foreach (var statBlock in statBlocks)
            {
                UpdateStatBlock(statBlock);
            }
        }
        public void UpdateStatBlock(StackPanel statBlock)
        {
            UpdateLayout();

            var visibilityMultiBinding =
                BindingOperations.GetMultiBindingExpression(statBlock,
                    VisibilityProperty);

            if (visibilityMultiBinding == null)
                return;

            foreach (var binding in
                visibilityMultiBinding.BindingExpressions)
            {
                binding.UpdateTarget();
            }

            visibilityMultiBinding.UpdateTarget();

            UpdateLayout();
        }

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                _loadingGridShowing = true;
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            _loadingGridShowing = false;
            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (_loadingGridShowing)
                        break;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        loadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    loadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            await UpdateProfile()
                .ConfigureAwait(true);
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            ImageBehavior.SetAnimatedSource(img, null);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        protected override async void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ViewModelPropertyChanged(sender, e);

            if (e.PropertyName.Length == 0)
            {
                await UpdateProfile()
                    .ConfigureAwait(true);
            }
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            ProfileStatPurpose
                .GetBindingExpression(UserProfileStat.StatValueProperty)?
                .UpdateTarget();
            ProfileStatSex
                .GetBindingExpression(UserProfileStat.StatValueProperty)?
                .UpdateTarget();
        }

        private async void OnCurrentUserChanged(object sender, UserChangedEventArgs e)
        {
            await UpdateProfile()
                .ConfigureAwait(true);
        }

        private void CopyUserLink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"memenim://app/showuserid/{ViewModel.CurrentProfileData.Id}");
        }

        private void CopyUserId_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.CurrentProfileData.Id.ToString());
        }

        private void btnEditMode_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatBlock(wpStatBlock1, wpStatBlock2,
                wpStatBlock3, wpStatBlock4);
        }

        private async void SelectAvatarFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string title = LocalizationUtils.GetLocalized("InsertingImageTitle");
            string message = LocalizationUtils.GetLocalized("EnterURL");

            string url = await DialogManager.ShowSinglelineTextDialog(
                    title, message)
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(url))
                return;

            await ProfileUtils.ChangeAvatar(url)
                .ConfigureAwait(true);

            ViewModel.CurrentProfileData.PhotoUrl = url;
        }

        private void SelectAvatarFromAnonymGallery_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
            {
                OnPicSelect = async url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    await ProfileUtils.ChangeAvatar(url)
                        .ConfigureAwait(true);

                    Dispatcher.Invoke(() =>
                    {
                        ViewModel.CurrentProfileData.PhotoUrl = url;
                    });
                }
            });
        }

        private void SelectAvatarFromTenor_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
            {
                OnPicSelect = async url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    await ProfileUtils.ChangeAvatar(url)
                        .ConfigureAwait(true);

                    Dispatcher.Invoke(() =>
                    {
                        ViewModel.CurrentProfileData.PhotoUrl = url;
                    });
                }
            });
        }

        private async void RemoveAvatar_Click(object sender, RoutedEventArgs e)
        {
            await ProfileUtils.RemoveAvatar()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            { 
                ViewModel.CurrentProfileData.PhotoUrl = string.Empty;
            });
        }

        private async void EditName_Click(object sender, RoutedEventArgs e)
        {
            UserProfilePage element = this;

            BindingExpression binding = element.txtName.GetBindingExpression(Emoji.Wpf.TextBlock.TextProperty);

            if (binding == null)
                return;

            ProfileSchema sourceClass = (ProfileSchema)binding.ResolvedSource;
            PropertyInfo sourceProperty = typeof(ProfileSchema).GetProperty(binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            string title = LocalizationUtils.GetLocalized("ProfileEditingTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");
            string statName = LocalizationUtils.GetLocalized("Nickname");

            string oldValue = (string)sourceProperty.GetValue(sourceClass);
            string value = await DialogManager.ShowSinglelineTextDialog(title,
                    $"{enterName} '{statName}'", oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);

                return;
            }

            ProfileUtils.OnNameChanged(this,
                new UserNameChangedEventArgs(oldValue, value, ViewModel.CurrentProfileData.Id));
        }

        private async void EditSinglelineText_Click(object sender, RoutedEventArgs e)
        {
            UserProfileStat element = sender as UserProfileStat;

            BindingExpression binding = element?.GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            ProfileSchema sourceClass = (ProfileSchema)binding.ResolvedSource;
            PropertyInfo sourceProperty = typeof(ProfileSchema).GetProperty(binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            string title = LocalizationUtils.GetLocalized("ProfileEditingTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");

            string oldValue = (string)sourceProperty.GetValue(sourceClass);
            string value = await DialogManager.ShowSinglelineTextDialog(title,
                    $"{enterName} '{element.StatTitle}'", oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }

            var statBlock = element.TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(statBlock);
        }

        private async void EditMultilineText_Click(object sender, RoutedEventArgs e)
        {
            UserProfileStat element = sender as UserProfileStat;

            BindingExpression binding = element?.GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            ProfileSchema sourceClass = (ProfileSchema)binding.ResolvedSource;
            PropertyInfo sourceProperty = typeof(ProfileSchema).GetProperty(binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            string title = LocalizationUtils.GetLocalized("ProfileEditingTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");

            string oldValue = (string)sourceProperty.GetValue(sourceClass);
            string value = await DialogManager.ShowMultilineTextDialog(title,
                    $"{enterName} '{element.StatTitle}'", oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }

            var statBlock = element.TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(statBlock);
        }

        private async void EditComboBoxPurpose_Click(object sender, RoutedEventArgs e)
        {
            UserProfileStat element = sender as UserProfileStat;

            BindingExpression binding = element?.GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            ProfileSchema sourceClass = (ProfileSchema)binding.ResolvedSource;
            PropertyInfo sourceProperty = typeof(ProfileSchema).GetProperty(binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            string title = LocalizationUtils.GetLocalized("ProfileEditingTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");

            ReadOnlyCollection<string> localizedNames =
                new ReadOnlyCollection<string>(UserPurposeType.Unknown.GetLocalizedNames());

            UserPurposeType oldValue = (UserPurposeType)(sourceProperty.GetValue(sourceClass) ?? UserPurposeType.Unknown);
            string valueName = await DialogManager.ShowComboBoxDialog(title,
                    $"{enterName} '{element.StatTitle}'", localizedNames,
                    oldValue.GetLocalizedName())
                .ConfigureAwait(true);

            if (valueName == null)
                return;

            UserPurposeType value = UserPurposeType.Unknown.ParseLocalizedName<UserPurposeType>(valueName);

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }

            var statBlock = element.TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(statBlock);
        }

        private async void EditComboBoxSex_Click(object sender, RoutedEventArgs e)
        {
            UserProfileStat element = sender as UserProfileStat;

            BindingExpression binding = element?.GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            ProfileSchema sourceClass = (ProfileSchema)binding.ResolvedSource;
            PropertyInfo sourceProperty = typeof(ProfileSchema).GetProperty(binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            string title = LocalizationUtils.GetLocalized("ProfileEditingTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");

            ReadOnlyCollection<string> localizedNames =
                new ReadOnlyCollection<string>(UserSexType.Unknown.GetLocalizedNames());

            UserSexType oldValue = (UserSexType)(sourceProperty.GetValue(sourceClass)
                                                 ?? UserSexType.Unknown);
            string valueName = await DialogManager.ShowComboBoxDialog(title,
                    $"{enterName} '{element.StatTitle}'", localizedNames,
                    oldValue.GetLocalizedName())
                .ConfigureAwait(true);

            if (valueName == null)
                return;

            UserSexType value = UserSexType.Unknown.ParseLocalizedName<UserSexType>(valueName);

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }

            var statBlock = element.TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(statBlock);
        }

        private async void EditNumericAge_Click(object sender, RoutedEventArgs e)
        {
            UserProfileStat element = sender as UserProfileStat;

            BindingExpression binding = element?.GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            ProfileSchema sourceClass = (ProfileSchema)binding.ResolvedSource;
            PropertyInfo sourceProperty = typeof(ProfileSchema).GetProperty(binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            string title = LocalizationUtils.GetLocalized("ProfileEditingTitle");
            string enterName = LocalizationUtils.GetLocalized("EnterTitle");

            int oldValue = (int)(sourceProperty.GetValue(sourceClass) ?? 0);
            double? value = await DialogManager.ShowNumericDialog(title,
                    $"{enterName} '{element.StatTitle}'", Convert.ToDouble(oldValue),
                    0.0, 255.0, 1.0, "F0")
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, Convert.ToInt32(value));

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }

            var statBlock = element.TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(statBlock);
        }

        private void Avatar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.CurrentProfileData.PhotoUrl))
                return;

            NavigationController.Instance.RequestOverlay<ImagePreviewOverlayPage>(new ImagePreviewOverlayViewModel()
            {
                ImageSource = ViewModel.CurrentProfileData.PhotoUrl
            });

            e.Handled = true;
        }

        private void Avatar_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!ViewModel.EditAllowed)
                e.Handled = true;
        }
    }
}

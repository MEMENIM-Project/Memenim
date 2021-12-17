using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
using RIS.Localization;
using WpfAnimatedGif;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class UserProfilePage : PageContent
    {
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register(nameof(IsEditMode), typeof(bool), typeof(UserProfilePage),
                new PropertyMetadata(false));



        private readonly SemaphoreSlim _profileUpdateLock;
        private int _profileUpdateWaitingCount;

        private bool _loadingGridShowing;



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



        public UserProfileViewModel ViewModel
        {
            get
            {
                return DataContext as UserProfileViewModel;
            }
        }



        public UserProfilePage()
        {
            _profileUpdateLock =
                new SemaphoreSlim(1, 1);
            _profileUpdateWaitingCount = 0;

            InitializeComponent();
            DataContext = new UserProfileViewModel();

            LocalizationUtils.LocalizationUpdated += OnLocalizationUpdated;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
        }

        ~UserProfilePage()
        {
            LocalizationUtils.LocalizationUpdated -= OnLocalizationUpdated;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
        }



        private async Task SelectAvatarImage(
            string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            await ProfileUtils.ChangeAvatar(url)
                .ConfigureAwait(true);

            ViewModel.CurrentProfileData.PhotoUrl = url;
        }



        public Task UpdateProfile()
        {
            return UpdateProfile(
                ViewModel.CurrentProfileData.Id);
        }
        public async Task UpdateProfile(
            int id)
        {
            await Dispatcher.Invoke(() =>
            {
                return ShowLoadingGrid();
            }).ConfigureAwait(false);

            try
            {
                Interlocked.Increment(
                    ref _profileUpdateWaitingCount);

                await _profileUpdateLock.WaitAsync()
                    .ConfigureAwait(true);
            }
            finally
            {
                Interlocked.Decrement(
                    ref _profileUpdateWaitingCount);
            }

            try
            {
                Dispatcher.Invoke(() =>
                {
                    EditModeButton.IsChecked = false;
                });

                if (id < 0)
                {
                    if (!NavigationController.Instance.IsCurrentContent<UserProfilePage>())
                        return;

                    NavigationController.Instance.GoBack(true);

                    var message = LocalizationUtils
                        .GetLocalized("UserNotFound");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                var result = await UserApi.GetProfileById(
                        id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    if (!NavigationController.Instance.IsCurrentContent<UserProfilePage>())
                        return;

                    NavigationController.Instance.GoBack(true);

                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (result.Data == null)
                {
                    if (!NavigationController.Instance.IsCurrentContent<UserProfilePage>())
                        return;

                    NavigationController.Instance.GoBack(true);

                    var message = LocalizationUtils
                        .GetLocalized("UserNotFound");

                    await DialogManager.ShowErrorDialog(message)
                        .ConfigureAwait(true);

                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    ViewModel.CurrentProfileData =
                        result.Data;

                    UpdateStatBlock(StatBlock1, StatBlock2,
                        StatBlock3, StatBlock4);
                });
            }
            finally
            {
                await Task.Delay(500)
                    .ConfigureAwait(true);

                if (_profileUpdateWaitingCount == 0)
                {
                    await Dispatcher.Invoke(() =>
                    {
                        return HideLoadingGrid();
                    }).ConfigureAwait(false);
                }

                _profileUpdateLock.Release();
            }
        }



        public void UpdateStatBlock(
            params StackPanel[] statBlocks)
        {
            foreach (var statBlock in statBlocks)
            {
                UpdateStatBlock(
                    statBlock);
            }
        }
        public void UpdateStatBlock(
            StackPanel statBlock)
        {
            UpdateLayout();

            var visibilityMultiBinding =
                BindingOperations.GetMultiBindingExpression(
                    statBlock,
                    VisibilityProperty);

            if (visibilityMultiBinding == null)
                return;

            foreach (var binding in
                visibilityMultiBinding.BindingExpressions)
            {
                binding
                    .UpdateTarget();
            }

            visibilityMultiBinding
                .UpdateTarget();

            UpdateLayout();
        }



        public void Share()
        {
            if (ViewModel.CurrentProfileData.Id == -1)
                return;

            var link = MemenimProtocolApiUtils.GetUserLink(
                ViewModel.CurrentProfileData.Id);

            Clipboard.SetText(link);
        }



        public Task ShowLoadingGrid()
        {
            _loadingGridShowing = true;
            LoadingIndicator.IsActive = true;
            LoadingGrid.Opacity = 1.0;
            LoadingGrid.IsHitTestVisible = true;
            LoadingGrid.Visibility = Visibility.Visible;

            return Task.CompletedTask;
        }

        public Task HideLoadingGrid()
        {
            _loadingGridShowing = false;
            LoadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (var i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (_loadingGridShowing)
                        break;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LoadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    LoadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }



        protected override async void OnEnter(object sender,
            RoutedEventArgs e)
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

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            ImageBehavior.SetAnimatedSource(
                Avatar.Image, null);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

        protected override async void ViewModelPropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            base.ViewModelPropertyChanged(sender, e);

            if (e.PropertyName.Length == 0)
            {
                await UpdateProfile()
                    .ConfigureAwait(true);
            }
        }



        private void OnLocalizationUpdated(object sender,
            LocalizationEventArgs e)
        {
            PurposeProfileStat
                .GetBindingExpression(UserProfileStat.StatValueProperty)?
                .UpdateTarget();
            SexProfileStat
                .GetBindingExpression(UserProfileStat.StatValueProperty)?
                .UpdateTarget();
        }

        private async void OnCurrentUserChanged(object sender,
            UserChangedEventArgs e)
        {
            await UpdateProfile()
                .ConfigureAwait(true);
        }



        private void CopyUserLink_Click(object sender,
            RoutedEventArgs e)
        {
            Share();
        }

        private void CopyUserId_Click(object sender,
            RoutedEventArgs e)
        {
            var id =
                ViewModel.CurrentProfileData.Id
                    .ToString();

            Clipboard.SetText(id);
        }

        private async void CopyUserNickname_Click(object sender,
            RoutedEventArgs e)
        {
            var nickname =
                ViewModel.CurrentProfileData.Nickname;

            if (nickname == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(nickname);
        }

        private async void CopyUserLogin_Click(object sender,
            RoutedEventArgs e)
        {
            var login =
                ViewModel.CurrentProfileData.Login;

            if (login == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(login);
        }

        private async void SelectAvatarFromUrl_Click(object sender,
            RoutedEventArgs e)
        {
            var title = LocalizationUtils
                .GetLocalized("InsertingImageTitle");
            var message = LocalizationUtils
                .GetLocalized("EnterUrl");

            var url = await DialogManager.ShowSinglelineTextDialog(
                    title, message)
                .ConfigureAwait(true);

            await SelectAvatarImage(url)
                .ConfigureAwait(true);
        }

        private void SelectAvatarFromAnonymGallery_Click(object sender,
            RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<AnonymGallerySearchPage>(new AnonymGallerySearchViewModel
            {
                ImageSelectionDelegate = SelectAvatarImage
            });
        }

        private void SelectAvatarFromTenor_Click(object sender,
            RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<TenorSearchPage>(new TenorSearchViewModel
            {
                ImageSelectionDelegate = SelectAvatarImage
            });
        }

        private async void RemoveAvatar_Click(object sender,
            RoutedEventArgs e)
        {
            await ProfileUtils.RemoveAvatar()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            { 
                ViewModel.CurrentProfileData.PhotoUrl = string.Empty;
            });
        }



        private async void EditName_Click(object sender,
            RoutedEventArgs e)
        {
            var page = this;

            var binding = page.NicknameTextBox
                .GetBindingExpression(Emoji.Wpf.TextBlock.TextProperty);

            if (binding == null)
                return;

            var sourceClass =
                (ProfileSchema)binding.ResolvedSource;
            var sourceProperty =
                typeof(ProfileSchema).GetProperty(
                    binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            var title = LocalizationUtils
                .GetLocalized("ProfileEditingTitle");
            var enterName = LocalizationUtils
                .GetLocalized("EnterTitle");
            var statName = LocalizationUtils
                .GetLocalized("Nickname");

            var oldValue =
                (string)sourceProperty.GetValue(
                    sourceClass);
            var value = await DialogManager.ShowSinglelineTextDialog(
                    title,
                    $"{enterName} '{statName}'",
                    oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            sourceProperty.SetValue(
                sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(
                    sourceClass, oldValue);

                return;
            }

            ProfileUtils.OnNameChanged(this,
                new UserNameChangedEventArgs(oldValue, value, ViewModel.CurrentProfileData.Id));
        }

        private async void EditSinglelineText_Click(object sender,
            RoutedEventArgs e)
        {
            var profileStat = sender as UserProfileStat;

            var binding = profileStat?
                .GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            var sourceClass =
                (ProfileSchema)binding.ResolvedSource;
            var sourceProperty =
                typeof(ProfileSchema).GetProperty(
                    binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            var title = LocalizationUtils
                .GetLocalized("ProfileEditingTitle");
            var enterName = LocalizationUtils
                .GetLocalized("EnterTitle");

            var oldValue =
                (string)sourceProperty.GetValue(
                    sourceClass);
            var value = await DialogManager.ShowSinglelineTextDialog(
                    title,
                    $"{enterName} '{profileStat.StatTitle}'",
                    oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            sourceProperty.SetValue(
                sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(
                    sourceClass, oldValue);
            }

            var statBlock = profileStat
                .TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(
                statBlock);
        }

        private async void EditMultilineText_Click(object sender,
            RoutedEventArgs e)
        {
            var profileStat = sender as UserProfileStat;

            var binding = profileStat?
                .GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            var sourceClass =
                (ProfileSchema)binding.ResolvedSource;
            var sourceProperty =
                typeof(ProfileSchema).GetProperty(
                    binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            var title = LocalizationUtils
                .GetLocalized("ProfileEditingTitle");
            var enterName = LocalizationUtils
                .GetLocalized("EnterTitle");

            var oldValue =
                (string)sourceProperty.GetValue(
                    sourceClass);
            var value = await DialogManager.ShowMultilineTextDialog(
                    title,
                    $"{enterName} '{profileStat.StatTitle}'",
                    oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            sourceProperty.SetValue(
                sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(
                    sourceClass, oldValue);
            }

            var statBlock = profileStat
                .TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(
                statBlock);
        }

        private async void EditComboBoxPurpose_Click(object sender,
            RoutedEventArgs e)
        {
            var profileStat = sender as UserProfileStat;

            var binding = profileStat?
                .GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            var sourceClass =
                (ProfileSchema)binding.ResolvedSource;
            var sourceProperty =
                typeof(ProfileSchema).GetProperty(
                    binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            var title = LocalizationUtils
                .GetLocalized("ProfileEditingTitle");
            var enterName = LocalizationUtils
                .GetLocalized("EnterTitle");

            var localizedNames =
                new ReadOnlyCollection<string>(
                    UserPurposeType.Unknown.GetLocalizedNames());

            var oldValue =
                (UserPurposeType)(sourceProperty.GetValue(sourceClass)
                                  ?? UserPurposeType.Unknown);
            var valueName = await DialogManager.ShowComboBoxDialog(
                    title,
                    $"{enterName} '{profileStat.StatTitle}'",
                    localizedNames,
                    oldValue.GetLocalizedName())
                .ConfigureAwait(true);

            if (valueName == null)
                return;

            var value = UserPurposeType.Unknown
                .ParseLocalizedName(valueName);

            sourceProperty.SetValue(
                sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(
                    sourceClass, oldValue);
            }

            var statBlock = profileStat
                .TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(
                statBlock);
        }

        private async void EditComboBoxSex_Click(object sender,
            RoutedEventArgs e)
        {
            var profileStat = sender as UserProfileStat;

            var binding = profileStat?
                .GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            var sourceClass =
                (ProfileSchema)binding.ResolvedSource;
            var sourceProperty =
                typeof(ProfileSchema).GetProperty(
                    binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            var title = LocalizationUtils
                .GetLocalized("ProfileEditingTitle");
            var enterName = LocalizationUtils
                .GetLocalized("EnterTitle");

            var localizedNames =
                new ReadOnlyCollection<string>(
                    UserSexType.Unknown.GetLocalizedNames());

            var oldValue =
                (UserSexType)(sourceProperty.GetValue(sourceClass)
                              ?? UserSexType.Unknown);
            var valueName = await DialogManager.ShowComboBoxDialog(
                    title,
                    $"{enterName} '{profileStat.StatTitle}'",
                    localizedNames,
                    oldValue.GetLocalizedName())
                .ConfigureAwait(true);

            if (valueName == null)
                return;

            var value = UserSexType.Unknown
                .ParseLocalizedName(valueName);

            sourceProperty.SetValue(
                sourceClass, value);

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(
                    sourceClass, oldValue);
            }

            var statBlock = profileStat
                .TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(
                statBlock);
        }

        private async void EditNumericAge_Click(object sender,
            RoutedEventArgs e)
        {
            var profileStat = sender as UserProfileStat;

            var binding = profileStat?
                .GetBindingExpression(UserProfileStat.StatValueProperty);

            if (binding == null)
                return;

            var sourceClass =
                (ProfileSchema)binding.ResolvedSource;
            var sourceProperty =
                typeof(ProfileSchema).GetProperty(
                    binding.ResolvedSourcePropertyName);

            if (sourceClass == null || sourceProperty == null)
                return;

            var title = LocalizationUtils
                .GetLocalized("ProfileEditingTitle");
            var enterName = LocalizationUtils
                .GetLocalized("EnterTitle");

            var oldValue =
                (int)(sourceProperty.GetValue(sourceClass)
                      ?? 0);
            var value = await DialogManager.ShowNumericDialog(
                    title,
                    $"{enterName} '{profileStat.StatTitle}'",
                    Convert.ToDouble(oldValue),
                    0.0,
                    255.0,
                    1.0,
                    "F0")
                .ConfigureAwait(true);

            if (value == null)
                return;

            sourceProperty.SetValue(
                sourceClass, Convert.ToInt32(value.Value));

            var request = await UserApi.EditProfile(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(
                    sourceClass, oldValue);
            }

            var statBlock = profileStat
                .TryFindParent<StackPanel>();

            if (statBlock == null)
                return;

            UpdateStatBlock(
                statBlock);
        }



        private void EditModeButton_Click(object sender,
            RoutedEventArgs e)
        {
            UpdateStatBlock(StatBlock1, StatBlock2,
                StatBlock3, StatBlock4);
        }

        private void Avatar_MouseLeftButtonUp(object sender,
            MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.CurrentProfileData.PhotoUrl))
                return;

            NavigationController.Instance.RequestOverlay<ImagePreviewOverlayPage>(new ImagePreviewOverlayViewModel
            {
                ImageSource = ViewModel.CurrentProfileData.PhotoUrl
            });

            e.Handled = true;
        }

        private void Avatar_MouseRightButtonUp(object sender,
            MouseButtonEventArgs e)
        {
            if (!ViewModel.EditAllowed)
                e.Handled = true;
        }
    }
}

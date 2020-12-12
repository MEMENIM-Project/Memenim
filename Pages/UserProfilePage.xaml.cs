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
using Memenim.Converters;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Extensions;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
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
        }

        public Task UpdateProfile()
        {
            return UpdateProfile(ViewModel.CurrentProfileData.id);
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
                    NavigationController.Instance.GoBack();

                    string notFoundLocalized = (string)MainWindow.Instance
                        .FindResource("UserNotFound");

                    await DialogManager.ShowDialog("Error", notFoundLocalized)
                        .ConfigureAwait(true);

                    return;
                }

                var result = await UserApi.GetProfileById(id)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    NavigationController.Instance.GoBack();

                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);

                    return;
                }

                if (result.data == null)
                {
                    NavigationController.Instance.GoBack();

                    string notFoundLocalized = (string)MainWindow.Instance
                        .FindResource("UserNotFound");

                    await DialogManager.ShowDialog("Error", notFoundLocalized)
                        .ConfigureAwait(true);

                    return;
                }

                ViewModel.CurrentProfileData = result.data;

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
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            await UpdateProfile()
                .ConfigureAwait(true);
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

        private void CopyUserLink_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"memenim://app/showuserid/{ViewModel.CurrentProfileData.id}");
        }

        private void CopyUserId_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ViewModel.CurrentProfileData.id.ToString());
        }

        private void AvatarImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.CurrentProfileData.photo))
                return;

            NavigationController.Instance.RequestOverlay<ImagePreviewOverlayPage>(new ImagePreviewOverlayViewModel()
            {
                ImageSource = ViewModel.CurrentProfileData.photo
            });

            e.Handled = true;
        }

        private async void SelectAvatarFromUrl_Click(object sender, RoutedEventArgs e)
        {
            string url = await DialogManager.ShowInputDialog("ENTER", "Enter pic URL")
                .ConfigureAwait(true);

            if (string.IsNullOrWhiteSpace(url))
                return;

            await ProfileUtils.ChangeAvatar(url)
                .ConfigureAwait(true);

            ViewModel.CurrentProfileData.photo = url;
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
                        ViewModel.CurrentProfileData.photo = url;
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
                        ViewModel.CurrentProfileData.photo = url;
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
                ViewModel.CurrentProfileData.photo = string.Empty;
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

            string enterLocalizeName = (string)MainWindow.Instance
                .FindResource("EnterTitle");

            string oldValue = (string)sourceProperty.GetValue(sourceClass);
            string value = await DialogManager.ShowSinglelineTextDialog("Edit profile",
                    $"{enterLocalizeName} Name", oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }
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

            string enterLocalizeName = (string)MainWindow.Instance
                .FindResource("EnterTitle");

            string oldValue = (string)sourceProperty.GetValue(sourceClass);
            string value = await DialogManager.ShowSinglelineTextDialog("Edit profile",
                    $"{enterLocalizeName} {element.StatTitle}", oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }
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

            string enterLocalizeName = (string)MainWindow.Instance
                .FindResource("EnterTitle");

            string oldValue = (string)sourceProperty.GetValue(sourceClass);
            string value = await DialogManager.ShowMultilineTextDialog("Edit profile",
                    $"{enterLocalizeName} {element.StatTitle}", oldValue)
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }
        }

        private void EditComboBoxPurpose_Click(object sender, RoutedEventArgs e)
        {

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

            string enterLocalizeName = (string)MainWindow.Instance
                .FindResource("EnterTitle");

            ReadOnlyCollection<string> localizedNames =
                new ReadOnlyCollection<string>(ProfileStatSexType.Unknown.GetLocalizedNames());

            int oldValue = (int)(sourceProperty.GetValue(sourceClass) ?? 0);
            string valueName = await DialogManager.ShowComboBoxDialog("Edit profile",
                    $"{enterLocalizeName} {element.StatTitle}", localizedNames,
                    ((ProfileStatSexType)((byte)oldValue)).GetLocalizedName())
                .ConfigureAwait(true);

            if (valueName == null)
                return;

            int value = (byte)ProfileStatSexType.Unknown.ParseLocalizedName<ProfileStatSexType>(valueName);

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, value);

            var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }
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

            string enterLocalizeName = (string)MainWindow.Instance
                .FindResource("EnterTitle");

            int oldValue = (int)(sourceProperty.GetValue(sourceClass) ?? 0);
            double? value = await DialogManager.ShowNumericDialog("Edit profile",
                    $"{enterLocalizeName} {element.StatTitle}", Convert.ToDouble(oldValue),
                    0.0, 255.0, 1.0, "F0")
                .ConfigureAwait(true);

            if (value == null)
                return;

            //sourceProperty.SetValue(sourceClass, value.Length == 0 ? null : value);
            sourceProperty.SetValue(sourceClass, Convert.ToInt32(value));

            var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                    ViewModel.CurrentProfileData)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);

                sourceProperty.SetValue(sourceClass, oldValue);
            }
        }
    }
}

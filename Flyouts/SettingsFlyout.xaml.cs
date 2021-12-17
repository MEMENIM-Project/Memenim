using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Extensions;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Settings;
using Memenim.SpecialEvents;
using Memenim.Utils;
using RIS.Localization;

namespace Memenim.Flyouts
{
    public partial class SettingsFlyout : FlyoutContent
    {
        public string SpecialEventLocalizedName
        {
            get
            {
                return SpecialEventManager.CurrentInstanceLocalizedName;
            }
        }
        private bool _specialEventEnabled;
        public bool SpecialEventEnabled
        {
            get
            {
                return _specialEventEnabled;
            }
            set
            {
                if (value
                    && !(SpecialEventManager.CurrentInstance is { EventLoaded: true }))
                {
                    SpecialEventToggle.IsOn = false;
                    SpecialEventToggle.IsEnabled = false;

                    return;
                }

                if (value)
                    SpecialEventManager.Activate();
                else
                    SpecialEventManager.Deactivate();

                _specialEventEnabled = value;
            }
        }
        private double _bgmVolume;
        public double BgmVolume
        {
            get
            {
                return _bgmVolume;
            }
            set
            {
                SpecialEventManager.SetVolume(value);

                _bgmVolume = value;
            }
        }
        public ReadOnlyDictionary<CommentReplyModeType, string> CommentReplyModes { get; private set; }
        public string AppVersion { get; private set; }



        public SettingsFlyout()
        {
            InitializeComponent();
            DataContext = this;

            AppVersion = $"v{SettingsManager.AppSettings.AppVersion}";

            UpdateCommentReplyModes();

            LocalizationUtils.LocalizationUpdated += OnLocalizationUpdated;
            SpecialEventManager.EventUpdated += OnSpecialEventUpdated;
        }

        ~SettingsFlyout()
        {
            LocalizationUtils.LocalizationUpdated -= OnLocalizationUpdated;
            SpecialEventManager.EventUpdated -= OnSpecialEventUpdated;
        }



        private void ReloadCommentReplyModes()
        {
            var names = Enum.GetNames(typeof(CommentReplyModeType));
            var localizedNames = CommentReplyModeType.Legacy.GetLocalizedNames();
            var postTypes = new Dictionary<CommentReplyModeType, string>(names.Length);

            for (var i = 0; i < names.Length; ++i)
            {
                postTypes.Add(
                    Enum.Parse<CommentReplyModeType>(names[i], true),
                    localizedNames[i]);
            }

            CommentReplyModeComboBox.SelectionChanged -= CommentReplyModeComboBox_SelectionChanged;

            KeyValuePair<CommentReplyModeType, string> selectedItem =
                new KeyValuePair<CommentReplyModeType, string>();

            if (CommentReplyModeComboBox.SelectedItem != null)
            {
                selectedItem =
                    (KeyValuePair<CommentReplyModeType, string>)CommentReplyModeComboBox.SelectedItem;
            }

            CommentReplyModes = new ReadOnlyDictionary<CommentReplyModeType, string>(postTypes);

            CommentReplyModeComboBox
                .GetBindingExpression(ItemsControl.ItemsSourceProperty)?
                .UpdateTarget();

            if (selectedItem.Value != null)
            {
                CommentReplyModeComboBox.SelectedItem =
                    new KeyValuePair<CommentReplyModeType, string>(selectedItem.Key, postTypes[selectedItem.Key]);
            }

            CommentReplyModeComboBox.SelectionChanged += CommentReplyModeComboBox_SelectionChanged;
        }

        private void UpdateCommentReplyModes()
        {
            if (LocalizationUtils.Localizations.Count == 0)
                return;

            ReloadCommentReplyModes();

            var commentReplyModeType = SettingsManager.AppSettings
                .CommentReplyModeEnum;

            CommentReplyModeComboBox.SelectedItem =
                new KeyValuePair<CommentReplyModeType, string>(
                    commentReplyModeType, CommentReplyModes[commentReplyModeType]);
        }

        private void UpdateSpecialEventName()
        {
            SpecialEventNameTextBlock
                .GetBindingExpression(TextBlock.TextProperty)?
                .UpdateTarget();
        }



        public void Show()
        {
            IsOpen = true;
        }

        public void Hide()
        {
            IsOpen = false;
        }



        private void OnLocalizationUpdated(object sender,
            LocalizationEventArgs e)
        {
            UpdateSpecialEventName();
            UpdateCommentReplyModes();
        }

        // ReSharper disable AccessToStaticMemberViaDerivedType
        private void OnSpecialEventUpdated(object sender,
            EventArgs e)
        {
            UpdateSpecialEventName();

            SpecialEventToggle
                .GetBindingExpression(ToggleSwitch.IsOnProperty)?
                .UpdateTarget();
            SpecialEventBgmSlider
                .GetBindingExpression(Slider.ValueProperty)?
                .UpdateTarget();
        }
        // ReSharper restore AccessToStaticMemberViaDerivedType



        private async void CommentReplyModeComboBox_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            var newReplyMode =
                ((KeyValuePair<CommentReplyModeType, string>)CommentReplyModeComboBox.SelectedItem)
                .Key;

            switch (newReplyMode)
            {
                case CommentReplyModeType.Experimental:
                    var additionalMessage = LocalizationUtils
                        .GetLocalized("YouMayBeBannedConfirmationMessage");
                    var confirmResult = await DialogManager.ShowConfirmationDialog(
                            additionalMessage)
                        .ConfigureAwait(true);

                    if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    {
                        CommentReplyModeComboBox.SelectionChanged -= CommentReplyModeComboBox_SelectionChanged;

                        if (e.RemovedItems.Count == 0 || e.RemovedItems[0] == null)
                        {
                            CommentReplyModeComboBox.SelectedItem =
                                new KeyValuePair<CommentReplyModeType, string>(
                                    CommentReplyModeType.Legacy, CommentReplyModes[CommentReplyModeType.Legacy]);

                            CommentReplyModeComboBox.SelectionChanged += CommentReplyModeComboBox_SelectionChanged;

                            break;
                        }

                        var oldReplyMode = ((KeyValuePair<CommentReplyModeType, string>)e.RemovedItems[0]).Key;

                        CommentReplyModeComboBox.SelectedItem =
                            new KeyValuePair<CommentReplyModeType, string>(
                                oldReplyMode, CommentReplyModes[oldReplyMode]);

                        CommentReplyModeComboBox.SelectionChanged += CommentReplyModeComboBox_SelectionChanged;
                    }

                    break;
                case CommentReplyModeType.Legacy:
                default:
                    break;
            }

            SettingsManager.AppSettings.CommentReplyModeEnum =
                ((KeyValuePair<CommentReplyModeType, string>)CommentReplyModeComboBox.SelectedItem).Key;

            SettingsManager.AppSettings.Save();
        }

        private async void ChangePasswordButton_Click(object sender,
            RoutedEventArgs e)
        {
            ChangePasswordButton.IsEnabled = false;

            try
            {
                var title = LocalizationUtils
                    .GetLocalized("ChangingPasswordTitle");
                var enterName = LocalizationUtils
                    .GetLocalized("EnterTitle");
                var oldPasswordName = LocalizationUtils
                    .GetLocalized("OldPassword");
                var newPasswordName = LocalizationUtils
                    .GetLocalized("NewPassword");

                var oldPassword = await DialogManager.ShowPasswordDialog(
                        title, $"{enterName} {oldPasswordName.ToLower()}",
                        false)
                    .ConfigureAwait(true);

                if (string.IsNullOrEmpty(oldPassword))
                    return;

                var newPassword = await DialogManager.ShowPasswordDialog(
                        title, $"{enterName} {newPasswordName.ToLower()}",
                        true)
                    .ConfigureAwait(true);

                if (string.IsNullOrEmpty(newPassword))
                    return;

                var request = await UserApi.ChangePassword(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        oldPassword, newPassword)
                    .ConfigureAwait(true);

                if (request.IsError)
                {
                    await DialogManager.ShowErrorDialog(request.Message)
                        .ConfigureAwait(true);
                }
            }
            finally
            {
                ChangePasswordButton.IsEnabled = true;
            }
        }

        private async void SignInToAnotherAccountButton_Click(object sender,
            RoutedEventArgs e)
        {
            SignInToAnotherAccountButton.IsEnabled = false;

            try
            {
                if (SettingsManager.PersistentSettings.CurrentUser.IsTemporary())
                {
                    var confirmResult = await DialogManager.ShowConfirmationDialog()
                        .ConfigureAwait(true);

                    if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    {
                        return;
                    }
                }

                SettingsManager.PersistentSettings.ResetCurrentUser();

                Hide();

                NavigationController.Instance.RequestPage<LoginPage>();
            }
            finally
            {
                SignInToAnotherAccountButton.IsEnabled = true;
            }
        }

        private async void SignOutAccountButton_Click(object sender,
            RoutedEventArgs e)
        {
            SignOutAccountButton.IsEnabled = false;

            try
            {
                var confirmResult = await DialogManager.ShowConfirmationDialog()
                    .ConfigureAwait(true);

                if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                {
                    return;
                }

                SettingsManager.PersistentSettings.RemoveUser(
                    SettingsManager.PersistentSettings.CurrentUser.Login);

                NavigationController.Instance.RequestPage<LoginPage>();
            }
            finally
            { 
                SignOutAccountButton.IsEnabled = true;
            }
        }

        private void OpenDiscordLinkButton_Click(object sender,
            RoutedEventArgs e)
        {
            const string link = "https://discord.gg/yhATVBWxZG";

            LinkUtils.OpenLink(link);
        }

        private void OpenTelegramLinkButton_Click(object sender,
            RoutedEventArgs e)
        {
            const string link = "https://t.me/joinchat/Vf9B3XM5SM-zUbkf";

            LinkUtils.OpenLink(link);
        }
    }
}

using System;
using System.Threading.Tasks;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Utils
{
    public static class ProfileUtils
    {
        public static event EventHandler<UserPhotoChangedEventArgs> AvatarChanged;
        public static event EventHandler<UserPhotoChangedEventArgs> BannerChanged;
        public static event EventHandler<UserNameChangedEventArgs> NameChanged;



        public static void OnAvatarChanged(object sender,
            UserPhotoChangedEventArgs e)
        {
            AvatarChanged?.Invoke(sender, e);
        }

        public static void OnBannerChanged(object sender,
            UserPhotoChangedEventArgs e)
        {
            BannerChanged?.Invoke(sender, e);
        }

        public static void OnNameChanged(object sender,
            UserNameChangedEventArgs e)
        {
            NameChanged?.Invoke(sender, e);
        }



        public static async Task ChangeAvatar(string url,
            bool ignoreEmptyUrl = true)
        {
            try
            {
                if (ignoreEmptyUrl && string.IsNullOrWhiteSpace(url))
                    return;

                var result = await UserApi.GetProfileById(
                        SettingsManager.PersistentSettings.CurrentUser.Id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);
                    return;
                }

                if (result.Data == null)
                {
                    var message = LocalizationUtils.GetLocalized("GettingProfileErrorMessage");

                    await DialogManager.ShowErrorDialog(
                            $"{message}: " + result.Message)
                        .ConfigureAwait(true);
                    return;
                }

                var oldPhoto = result.Data.PhotoUrl;
                result.Data.PhotoUrl = url;

                var request = await UserApi.EditProfile(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        result.Data)
                    .ConfigureAwait(true);

                if (request.IsError)
                {
                    await DialogManager.ShowErrorDialog(request.Message)
                        .ConfigureAwait(true);
                    return;
                }

                OnAvatarChanged(null,
                    new UserPhotoChangedEventArgs(oldPhoto, result.Data.PhotoUrl, result.Data.Id));
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
        }

        public static Task RemoveAvatar()
        {
            return ChangeAvatar(string.Empty, false);
        }


        public static async Task ChangeBanner(string url,
            bool ignoreEmptyUrl = true)
        {
            try
            {
                if (ignoreEmptyUrl && string.IsNullOrWhiteSpace(url))
                    return;

                var result = await UserApi.GetProfileById(
                        SettingsManager.PersistentSettings.CurrentUser.Id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);
                    return;
                }

                if (result.Data == null)
                {
                    var message = LocalizationUtils.GetLocalized("GettingProfileErrorMessage");

                    await DialogManager.ShowErrorDialog(
                            $"{message}: " + result.Message)
                        .ConfigureAwait(true);
                    return;
                }

                var oldPhoto = result.Data.BannerUrl;
                result.Data.BannerUrl = url;

                var request = await UserApi.EditProfile(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        result.Data)
                    .ConfigureAwait(true);

                if (request.IsError)
                {
                    await DialogManager.ShowErrorDialog(request.Message)
                        .ConfigureAwait(true);
                    return;
                }

                OnBannerChanged(null,
                    new UserPhotoChangedEventArgs(oldPhoto, result.Data.BannerUrl, result.Data.Id));
            }
            catch (Exception ex)
            {
                await DialogManager.ShowErrorDialog(ex.Message)
                    .ConfigureAwait(true);
            }
        }

        public static Task RemoveBanner()
        {
            return ChangeBanner(string.Empty, false);
        }
    }
}

using System;
using System.Threading.Tasks;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Utils
{
    public static class ProfileUtils
    {
        public static async Task ChangeAvatar(string url,
            bool checkEmptyUrl = true)
        {
            try
            {
                if (checkEmptyUrl && string.IsNullOrWhiteSpace(url))
                    return;

                var result = await UserApi.GetProfileById(
                        SettingsManager.PersistentSettings.CurrentUser.Id)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                    return;
                }

                if (result.data == null)
                {
                    await DialogManager.ShowDialog("F U C K",
                            "error getting a profile. " + result.message)
                        .ConfigureAwait(true);
                    return;
                }

                result.data.photo = url;

                var request = await UserApi.EditProfile(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        result.data)
                    .ConfigureAwait(true);

                if (request.error)
                {
                    await DialogManager.ShowDialog("F U C K", request.message)
                        .ConfigureAwait(true);
                }

            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("F U C K", ex.Message)
                    .ConfigureAwait(true);
            }
        }

        public static Task RemoveAvatar()
        {
            return ChangeAvatar(string.Empty, false);
        }

        public static async Task ChangeBanner(string url,
            bool checkEmptyUrl = true)
        {
            try
            {
                if (checkEmptyUrl && string.IsNullOrWhiteSpace(url))
                    return;

                var result = await UserApi.GetProfileById(
                        SettingsManager.PersistentSettings.CurrentUser.Id)
                    .ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                    return;
                }

                if (result.data == null)
                {
                    await DialogManager.ShowDialog("F U C K",
                            "error getting a profile. " + result.message)
                        .ConfigureAwait(true);
                    return;
                }

                result.data.banner = url;

                var request = await UserApi.EditProfile(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        result.data)
                    .ConfigureAwait(true);

                if (request.error)
                {
                    await DialogManager.ShowDialog("F U C K", request.message)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("F U C K", ex.Message)
                    .ConfigureAwait(true);
            }
        }

        public static Task RemoveBanner()
        {
            return ChangeBanner(string.Empty, false);
        }
    }
}

using System;
using System.Threading.Tasks;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Utils
{
    public static class ProfileUtils
    {
        public static async Task ChangeAvatar(string url)
        {
            try
            {
                var result = await UserApi.GetProfileById(SettingsManager.PersistentSettings.CurrentUserId)
                    .ConfigureAwait(true);

                if (!result.error)
                {
                    if (result.data == null)
                    {
                        await DialogManager.ShowDialog("F U C K", result.message)
                            .ConfigureAwait(true);
                        return;
                    }

                    result.data.photo = url;

                    var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                            result.data)
                        .ConfigureAwait(true);

                    if (!request.error)
                    {
                        await DialogManager.ShowDialog("S U C C", "You changed your avatar.")
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        await DialogManager.ShowDialog("F U C K", request.message)
                            .ConfigureAwait(true);
                    }
                }
                else
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("F U C K", ex.Message)
                    .ConfigureAwait(true);
            }
        }

        public static async Task ChangeBanner(string url)
        {
            try
            {
                var result = await UserApi.GetProfileById(SettingsManager.PersistentSettings.CurrentUserId)
                    .ConfigureAwait(true);

                if (!result.error)
                {
                    if (result.data == null)
                    {
                        await DialogManager.ShowDialog("F U C K", result.message)
                            .ConfigureAwait(true);
                        return;
                    }

                    result.data.banner = url;

                    var request = await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUserToken,
                            result.data)
                        .ConfigureAwait(true);

                    if (!request.error)
                    {
                        await DialogManager.ShowDialog("S U C C", "You changed your banner.")
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        await DialogManager.ShowDialog("F U C K", request.message)
                            .ConfigureAwait(true);
                    }
                }
                else
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("F U C K", ex.Message)
                    .ConfigureAwait(true);
            }
        }
    }
}

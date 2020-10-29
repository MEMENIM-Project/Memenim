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
                    result.data[0].photo = url;

                    var request = await UserApi.EditProfile(result.data[0], SettingsManager.PersistentSettings.CurrentUserToken)
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
                    result.data[0].banner = url;

                    var request = await UserApi.EditProfile(result.data[0], SettingsManager.PersistentSettings.CurrentUserToken)
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

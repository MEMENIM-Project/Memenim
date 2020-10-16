using System;
using System.Threading.Tasks;
using Memenim.Core.Api;

namespace Memenim.Utils
{
    static class ProfileUtils
    {
        public static async Task ChangeAvatar(string Url)
        {
            try
            {
                var profile = await UserApi.GetProfileById(AppPersistent.LocalUserId);
                if (!profile.error)
                {
                    profile.data[0].photo = Url;
                    var res = await UserApi.EditProfile(profile.data[0], AppPersistent.UserToken);
                    if (!res.error)
                    {
                        DialogManager.ShowDialog("S U C C", "You changed your avatar.");
                    }
                    else
                    {
                        DialogManager.ShowDialog("F U C K", res.message);
                    }
                }
                else
                {
                    DialogManager.ShowDialog("F U C K", profile.message);
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("F U C K", ex.Message);
            }
        }

        public static async Task ChangeBanner(string Url)
        {
            try
            {
                var profile = await UserApi.GetProfileById(AppPersistent.LocalUserId);
                if (!profile.error)
                {
                    profile.data[0].banner = Url;
                    var res = await UserApi.EditProfile(profile.data[0], AppPersistent.UserToken);
                    if (!res.error)
                    {
                        DialogManager.ShowDialog("S U C C", "You changed your banner.");
                    }
                    else
                    {
                        DialogManager.ShowDialog("F U C K", res.message);
                    }
                }
                else
                {
                    DialogManager.ShowDialog("F U C K", profile.message);
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("F U C K", ex.Message);
            }

        }

    }
}

using System;
using System.Linq;
using Memenim.Core.Schema;
using Memenim.Logs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using RIS.Reflection.Call;

namespace Memenim.Protocols.Schemas
{
    public class MemenimSchema : IUserProtocolSchema
    {
        private static Calls<MemenimSchema> SchemaCalls { get; }

        public string Name { get; }

        static MemenimSchema()
        {
            SchemaCalls = new Calls<MemenimSchema>();
        }

        public MemenimSchema()
        {
            Name = "memenim";
        }

        public bool ParseUri(string uriString)
        {
            try
            {
                if (string.IsNullOrEmpty(uriString)
                    || string.IsNullOrWhiteSpace(Name))
                {
                    return false;
                }

                if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri)
                    || uri.Scheme != Name)
                {
                    return false;
                }

                var requestComponents = uri.Segments
                    .Select(segment => segment.Trim(' ', '/'))
                    .Where(segment => !string.IsNullOrEmpty(segment))
                    .ToArray();

                LogManager.DebugLog.Info($"Request components - {string.Join(',', requestComponents)}");

                if (requestComponents.Length == 2)
                {
                    string methodName = requestComponents[0].Length < 2
                        ? requestComponents[0].ToUpperInvariant()
                        : requestComponents[0][0].ToString().ToUpperInvariant()
                          + requestComponents[0][1..].ToLowerInvariant();

                    LogManager.DebugLog.Info($"Request method -  name={methodName},args={requestComponents[1]}");

                    return SchemaCalls.CallStaticMethod(methodName,
                        requestComponents[1]);
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Log.Error(ex, "Schema parse uri error");
                return false;
            }
        }



        // ReSharper disable IdentifierTypo

        private static bool Showuserid(string args)
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUserLogin))
                    return false;

                if (!int.TryParse(args, out var id))
                    return false;

                if (id < 0)
                    return false;

                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
                    {
                        CurrentProfileData = new ProfileSchema
                        {
                            id = id
                        }
                    });
                });

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log.Error(ex, "Schema parse uri error");
                return false;
            }
        }

        private static bool Showpostid(string args)
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUserLogin))
                    return false;

                if (!int.TryParse(args, out var id))
                    return false;

                if (id < 0)
                    return false;

                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    NavigationController.Instance.RequestPage<FeedPage>();

                    NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayViewModel
                    {
                        CurrentPostData = new PostSchema()
                        {
                            id = id
                        }
                    });
                });

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log.Error(ex, "Schema parse uri error");
                return false;
            }
        }

        // ReSharper enable IdentifierTypo
    }
}

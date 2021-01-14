using System;
using System.Collections.Generic;
using System.Linq;
using Memenim.Core.Schema;
using Memenim.Logs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Widgets;
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
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUser.Login))
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
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUser.Login))
                    return false;

                if (!int.TryParse(args, out var id))
                    return false;

                if (id < 0)
                    return false;

                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    NavigationController.Instance.RequestPage<FeedPage>();

                    PostWidget sourcePost = null;
                    FeedPage page = (FeedPage)PageStorage.GetPage<FeedPage>();

                    if (page != null)
                    {
                        var postType = ((KeyValuePair<PostType, string>)page.slcPostTypes.SelectedItem).Key;

                        switch (postType)
                        {
                            case PostType.Popular:
                                if (page.lstPosts.Children.Count == 0)
                                    break;

                                foreach (var element in page.lstPosts.Children)
                                {
                                    if (!(element is PostWidget post))
                                        continue;

                                    if (post.CurrentPostData.id != id)
                                        continue;

                                    sourcePost = post;
                                    break;
                                }

                                break;
                            case PostType.New:
                            case PostType.My:
                            case PostType.Favorite:
                                if (page.lstPosts.Children.Count == 0)
                                    break;

                                if (page.lstPosts.Children.Count > 2
                                    && (page.lstPosts.Children[0] is PostWidget startPost
                                        && page.lstPosts.Children[^1] is PostWidget endPost)
                                    && !(startPost.CurrentPostData.id >= id
                                         && id >= endPost.CurrentPostData.id))
                                {
                                    break;
                                }

                                foreach (var element in page.lstPosts.Children)
                                {
                                    if (!(element is PostWidget post))
                                        continue;

                                    if (post.CurrentPostData.id != id)
                                        continue;

                                    sourcePost = post;
                                    break;
                                }

                                break;
                            default:
                                break;
                        }
                    }

                    NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayViewModel
                    {
                        SourcePostWidget = sourcePost,
                        CurrentPostData = new PostSchema
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

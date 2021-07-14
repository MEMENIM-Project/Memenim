using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
using RIS.Logging;
using RIS.Reflection.Mapping;

namespace Memenim.Protocols.Schemas
{
    public class MemenimSchema : IUserProtocolSchema
    {
        private readonly MethodMap<MemenimSchema> _schemaMap;

        public string Name { get; }



        public MemenimSchema()
        {
            _schemaMap = new MethodMap<MemenimSchema>(
                this,
                new[]
                {
                    typeof(string)
                },
                typeof(bool));
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

                if (requestComponents.Length >= 2)
                {
                    string methodName = string.Join('/',
                        requestComponents[..^1]);
                    string args = requestComponents[^1];

                    LogManager.DebugLog.Info($"Request method - Name={methodName},Args={args}");

                    return _schemaMap.Invoke<bool>(
                        methodName, args);
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Log.Error(ex, "Schema parse uri error");

                return false;
            }
        }



        // ReSharper disable UnusedMember.Local

        [MappedMethod("user/id")]
        [MappedMethod("showuserid")]
        private static bool ShowUserById(string args)
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
                            Id = id
                        }
                    });
                });

                return true;
            }
            catch (Exception ex)
            {
                var method = MethodBase.GetCurrentMethod();

                ProtocolSchemaUtils.LogMethodError(
                    ex, method, args);

                return false;
            }
        }



        [MappedMethod("post/id")]
        [MappedMethod("showpostid")]
        private static bool ShowPostById(string args)
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUser.Login))
                    return false;

                if (!int.TryParse(args, out var id))
                    return false;

                if (id < 0)
                    return false;

                var result = MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    if (!NavigationController.Instance.IsCurrentPage<FeedPage>())
                    {
                        NavigationController.Instance.RequestPage<FeedPage>(
                            null, true);
                    }

                    PostWidget sourcePost = null;
                    var page = (FeedPage)PageStorage.GetPage<FeedPage>();

                    var slcPostTypes = page?.slcPostTypes;

                    if (slcPostTypes?.SelectedItem == null)
                        return false;

                    var postType = ((KeyValuePair<PostType, string>)slcPostTypes.SelectedItem).Key;

                    switch (postType)
                    {
                        case PostType.Popular:
                            if (page.lstPosts.Children.Count == 0)
                                break;

                            foreach (var element in page.lstPosts.Children)
                            {
                                if (!(element is PostWidget post))
                                    continue;

                                if (post.CurrentPostData.Id != id)
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
                                && !(startPost.CurrentPostData.Id >= id
                                     && id >= endPost.CurrentPostData.Id))
                            {
                                break;
                            }

                            foreach (var element in page.lstPosts.Children)
                            {
                                if (!(element is PostWidget post))
                                    continue;

                                if (post.CurrentPostData.Id != id)
                                    continue;

                                sourcePost = post;
                                break;
                            }

                            break;
                        default:
                            break;
                    }

                    NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayViewModel
                    {
                        SourcePostWidget = sourcePost,
                        CurrentPostData = new PostSchema
                        {
                            Id = id
                        }
                    });

                    return true;
                });

                if (!result)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                var method = MethodBase.GetCurrentMethod();

                ProtocolSchemaUtils.LogMethodError(
                    ex, method, args);

                return false;
            }
        }



        [MappedMethod("hash/files")]
        private static bool ManageHashFiles(string args)
        {
            try
            {
                if (string.IsNullOrEmpty(args))
                    return false;

                switch (args)
                {
                    case "create":
                        App.CreateHashFiles();

                        break;
                    default:
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                var method = MethodBase.GetCurrentMethod();

                ProtocolSchemaUtils.LogMethodError(
                    ex, method, args);

                return false;
            }
        }

        // ReSharper restore UnusedMember.Local
    }
}

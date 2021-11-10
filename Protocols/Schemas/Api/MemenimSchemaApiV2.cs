using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using Memenim.Core.Schema;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
using RIS.Logging;
using RIS.Reflection.Mapping;

namespace Memenim.Protocols.Schemas.Api
{
    public sealed class MemenimSchemaApiV2 : IUserProtocolSchemaApi
    {
        public const uint StaticVersion = 2;
        public const string StaticSchemaName = MemenimSchema.StaticName;



        private readonly StaticMethodMap _map;

        public uint Version
        {
            get
            {
                return StaticVersion;
            }
        }
        public string SchemaName
        {
            get
            {
                return StaticSchemaName;
            }
        }



        private MemenimSchemaApiV2()
        {
            _map = new StaticMethodMap(
                typeof(MemenimSchemaApiV2),
                new[]
                {
                    typeof(NameValueCollection)
                },
                typeof(bool));
        }



#pragma warning disable U2U1200 // Prefer generic collections over non-generic ones
        public bool ParseUri(Uri uri)
        {
            try
            {
                if (uri == null
                    || !uri.IsAbsoluteUri
                    || uri.Scheme != SchemaName
                    || uri.Host != "app")
                {
                    return false;
                }

                var path = uri.AbsolutePath
                    .TrimStart('/');
                var query = uri.Query
                    .TrimStart('?');

                LogManager.Debug.Info(
                    $"Receive api[SchemaName = {SchemaName}, Version = {Version}] request - Path = '{path}', Query = '{query}'");

                if (string.IsNullOrEmpty(path))
                    return false;

                NameValueCollection args = null;

                if (!string.IsNullOrEmpty(query))
                {
                    args = HttpUtility.ParseQueryString(
                        query);
                }

                return _map.Invoke<bool>(
                    path, args);
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    $"User protocol schema api[SchemaName = {SchemaName}, Version = {Version}] parse uri error");

                return false;
            }
        }
#pragma warning restore U2U1200 // Prefer generic collections over non-generic ones



        // ReSharper disable UnusedMember.Local

        [MappedMethod("user/show")]
        private static bool ShowUserById(NameValueCollection args)
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUser.Login))
                    return false;

                var idArg = args.Get("id");

                if (string.IsNullOrEmpty(idArg))
                    return false;
                if (!int.TryParse(idArg, out var id))
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

                ProtocolSchemaUtils.LogApiMethodError(
                    ex, method, StaticSchemaName,
                    StaticVersion, args);

                return false;
            }
        }



        [MappedMethod("post/show")]
        private static bool ShowPostById(NameValueCollection args)
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsManager.PersistentSettings.CurrentUser.Login))
                    return false;

                var idArg = args.Get("id");

                if (string.IsNullOrEmpty(idArg))
                    return false;
                if (!int.TryParse(idArg, out var id))
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

                ProtocolSchemaUtils.LogApiMethodError(
                    ex, method, StaticSchemaName,
                    StaticVersion, args);

                return false;
            }
        }



        [MappedMethod("hash/files/create")]
        private static bool CreateHashFiles(NameValueCollection args)
        {
            try
            {
                App.CreateHashFiles();

                return true;
            }
            catch (Exception ex)
            {
                var method = MethodBase.GetCurrentMethod();

                ProtocolSchemaUtils.LogApiMethodError(
                    ex, method, StaticSchemaName,
                    StaticVersion, args);

                return false;
            }
        }

        // ReSharper restore UnusedMember.Local
    }
}

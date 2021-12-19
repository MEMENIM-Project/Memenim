using System;
using System.Collections.Generic;
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

namespace Memenim.Protocols.Schemas.Api
{
    public sealed class MemenimSchemaApiV1 : IUserProtocolSchemaApi
    {
        public const uint StaticVersion = 1;
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



        private MemenimSchemaApiV1()
        {
            _map = new StaticMethodMap(
                typeof(MemenimSchemaApiV1),
                new[]
                {
                    typeof(string)
                },
                typeof(bool));
        }



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

                LogManager.Debug.Info(
                    $"Receive api[SchemaName = {SchemaName}, Version = {Version}] request - Path = '{path}'");

                if (string.IsNullOrEmpty(path))
                    return false;

                var segments = uri.Segments;

                if (segments.Length < 3)
                    return false;

                var methodPath = string
                    .Join(string.Empty,
                        segments[1..^1])
                    .TrimEnd('/');
                var args = segments[^1];

                return _map.Invoke<bool>(
                    methodPath, args);
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    $"User protocol schema api[SchemaName = {SchemaName}, Version = {Version}] parse uri error");

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

                ProtocolSchemaUtils.LogApiMethodError(
                    ex, method, StaticSchemaName,
                    StaticVersion, args);

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

                    Post sourcePost = null;
                    var page = (FeedPage)PageStorage.GetPage<FeedPage>();

                    if (page.PostsTypesComboBox.SelectedItem == null)
                        return false;

                    var postType =
                        ((KeyValuePair<PostType, string>)page.PostsTypesComboBox.SelectedItem)
                        .Key;

                    switch (postType)
                    {
                        case PostType.Popular:
                            if (page.PostsWrapPanel.Children.Count == 0)
                                break;

                            foreach (var element in page.PostsWrapPanel.Children)
                            {
                                if (!(element is Post post))
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
                            if (page.PostsWrapPanel.Children.Count == 0)
                                break;

                            if (page.PostsWrapPanel.Children.Count > 2
                                && (page.PostsWrapPanel.Children[0] is Post startPost
                                    && page.PostsWrapPanel.Children[^1] is Post endPost)
                                && !(startPost.CurrentPostData.Id >= id
                                     && id >= endPost.CurrentPostData.Id))
                            {
                                break;
                            }

                            foreach (var element in page.PostsWrapPanel.Children)
                            {
                                if (!(element is Post post))
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
                        SourcePost = sourcePost,
                        CurrentPostData = new PostSchema
                        {
                            Id = id
                        }
                    });

                    return true;
                });

                return result;
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

                ProtocolSchemaUtils.LogApiMethodError(
                    ex, method, StaticSchemaName,
                    StaticVersion, args);

                return false;
            }
        }

        // ReSharper restore UnusedMember.Local
    }
}

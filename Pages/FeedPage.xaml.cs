using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Memenim.Commands;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Extensions;
using Memenim.Generic;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Utils;
using Memenim.Widgets;
using RIS.Localization;
using WpfAnimatedGif;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class FeedPage : PageContent
    {
        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.Register(nameof(IsEmpty), typeof(bool), typeof(FeedPage),
                new PropertyMetadata(true));



        private const int OffsetPerTime = 20;



        // ReSharper disable InconsistentNaming
        private readonly Storyboard LoadingMoreShowAnimation;
        private readonly Storyboard LoadingMoreHideAnimation;
        // ReSharper restore InconsistentNaming

        private readonly Timer _autoUpdateCountTimer;



        public bool IsEmpty
        {
            get
            {
                return (bool)GetValue(IsEmptyProperty);
            }
            private set
            {
                SetValue(IsEmptyProperty, value);
            }
        }
        public ReadOnlyDictionary<PostType, string> PostTypes { get; private set; }



        public FeedViewModel ViewModel
        {
            get
            {
                return DataContext as FeedViewModel;
            }
        }



        public FeedPage()
        {
            InitializeComponent();
            DataContext = new FeedViewModel();

            LoadingMoreShowAnimation = (Storyboard)FindResource(
                nameof(LoadingMoreShowAnimation));
            LoadingMoreHideAnimation = (Storyboard)FindResource(
                nameof(LoadingMoreHideAnimation));

            _autoUpdateCountTimer = new Timer
            {
                Interval = TimeSpan
                    .FromSeconds(60)
                    .TotalMilliseconds
            };
            _autoUpdateCountTimer.Elapsed += AutoUpdateCountTimer_Tick;

            UpdatePostsTypes();

            LocalizationUtils.LocalizationUpdated += OnLocalizationUpdated;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
        }

        ~FeedPage()
        {
            LocalizationUtils.LocalizationUpdated -= OnLocalizationUpdated;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
        }



        private void ReloadPostsTypes()
        {
            var names = Enum.GetNames(
                typeof(PostType));
            var localizedNames = PostType.New
                .GetLocalizedNames();
            var postsTypes = new Dictionary<PostType, string>(
                names.Length);

            for (var i = 0; i < names.Length; ++i)
            {
                postsTypes.Add(
                        Enum.Parse<PostType>(
                            names[i], true),
                        localizedNames[i]);
            }

            PostsTypesComboBox.SelectionChanged -= PostsTypesComboBox_SelectionChanged;

            var selectedItem =
                new KeyValuePair<PostType, string>();

            if (PostsTypesComboBox.SelectedItem != null)
            {
                selectedItem =
                    (KeyValuePair<PostType, string>)PostsTypesComboBox.SelectedItem;
            }

            PostTypes =
                new ReadOnlyDictionary<PostType, string>(
                    postsTypes);

            PostsTypesComboBox
                .GetBindingExpression(ItemsControl.ItemsSourceProperty)?
                .UpdateTarget();

            if (selectedItem.Value != null)
            {
                PostsTypesComboBox.SelectedItem =
                    new KeyValuePair<PostType, string>(
                        selectedItem.Key, postsTypes[selectedItem.Key]);
            }

            PostsTypesComboBox.SelectionChanged += PostsTypesComboBox_SelectionChanged;
        }

        private void UpdatePostsTypes()
        {
            if (LocalizationUtils.Localizations.Count == 0)
                return;

            ReloadPostsTypes();

            if (PostsTypesComboBox.SelectedItem == null)
            {
                var postsType = SettingsManager.AppSettings
                    .PostsTypeEnum;

                PostsTypesComboBox.SelectedItem =
                    new KeyValuePair<PostType, string>(
                        postsType, PostTypes[postsType]);
            }
        }



        public Task UpdatePosts()
        {
            return UpdatePosts(
                ((KeyValuePair<PostType, string>)PostsTypesComboBox.SelectedItem)
                .Key);
        }
        public async Task UpdatePosts(
            PostType type)
        {
            _autoUpdateCountTimer.Stop();

            PostsTypesComboBox.IsEnabled = false;
            RefreshPostsButton.IsEnabled = false;

            await ShowLoadingGrid()
                .ConfigureAwait(true);

            ViewModel.NewPostsCount = 0;

            PostsScrollViewer.IsEnabled = false;

            try
            {
                foreach (var element in PostsWrapPanel.Children)
                {
                    if (!(element is Post post))
                        continue;

                    ImageBehavior.SetAnimatedSource(
                        post.Image, null);
                }

                PostsWrapPanel.Children.Clear();
                PostsScrollViewer
                    .ScrollToHorizontalOffset(0);

                ViewModel.Offset = 0;

                IsEmpty = true;

                UpdateLayout();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                await LoadMorePosts(type)
                    .ConfigureAwait(true);

                if (PostsWrapPanel.Children.Count == 0)
                {
                    ViewModel.LastNewHeadPostId = -1;
                }
                else
                {
                    ViewModel.LastNewHeadPostId =
                        (PostsWrapPanel.Children[0] as Post)?
                        .CurrentPostData.Id ?? -1;

                    IsEmpty = false;
                }
            }
            finally
            {
                PostsScrollViewer.IsEnabled = true;

                await HideLoadingGrid()
                    .ConfigureAwait(true);

                RefreshPostsButton.IsEnabled = true;
                PostsTypesComboBox.IsEnabled = true;

                _autoUpdateCountTimer.Start();
            }
        }



        public Task LoadMorePosts()
        {
            return LoadMorePosts(
                ((KeyValuePair<PostType, string>)PostsTypesComboBox.SelectedItem)
                .Key);
        }
        public async Task LoadMorePosts(
            PostType type)
        {
            if (PostsWrapPanel.Children.Count != 0)
            {
                _autoUpdateCountTimer.Stop();

                var newPostsCount = await GetNewPostsCount()
                    .ConfigureAwait(true);

                ViewModel.Offset += newPostsCount;
                ViewModel.NewPostsCount += newPostsCount;

                _autoUpdateCountTimer.Start();
            }

            await LoadMorePosts(
                    type, ViewModel.Offset)
                .ConfigureAwait(true);
        }
        public Task LoadMorePosts(
            PostType type, int offset)
        {
            return LoadMorePosts(
                type, OffsetPerTime, offset);
        }
        public async Task LoadMorePosts(
            PostType type, int count, int offset)
        {
            ShowLoadingMoreGrid();

            try
            {
                var result = await PostApi.Get(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        type, count, offset)
                    .ConfigureAwait(true);

                if (result == null)
                    return;

                result.Data ??= new List<PostSchema>();

                await AddMorePosts(result.Data)
                    .ConfigureAwait(true);
            }
            finally
            {
                HideLoadingMoreGrid();
            }
        }

        public Task AddMorePosts(List<PostSchema> posts)
        {
            return Task.Run(async () =>
            {
                foreach (var post in posts)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        var postWidget = new Post
                        {
                            CurrentPostData = post,
                            TextSizeLimit = true,
                            Margin = new Thickness(5)
                        };
                        postWidget.Click += Post_Click;
                        postWidget.PostDelete += Post_Delete;

                        PostsWrapPanel.Children.Add(
                            postWidget);
                    }).Task.ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    ViewModel.Offset += posts.Count;
                });
            });
        }



        public Task<int> GetNewPostsCount(
            int offset = 0)
        {
            var postType = PostType.Popular;

            Dispatcher.Invoke(() =>
            {
                postType =
                    ((KeyValuePair<PostType, string>)PostsTypesComboBox.SelectedItem)
                    .Key;
            });

            return GetNewPostsCount(
                postType, OffsetPerTime, offset);
        }
        public async Task<int> GetNewPostsCount(
            PostType type, int countPerTime, int offset)
        {
            var postsCount = 0;

            Dispatcher.Invoke(() =>
            {
                postsCount = PostsWrapPanel.Children.Count;
            });

            if (postsCount == 0)
            {
                if (type == PostType.My || type == PostType.Favorite)
                {
                    return await GetAllPostsCount(
                            type, countPerTime, offset)
                        .ConfigureAwait(true);
                }

                return 0;
            }

            var headOldId = -1;

            Dispatcher.Invoke(() =>
            {
                headOldId = ViewModel.LastNewHeadPostId;
            });

            if (headOldId == -1)
                return 0;

            return await Task.Run(async () =>
            {
                var countNew = 0;
                var headNewId = -1;
                var headOldIsFound = false;
                var isFirstRequest = true;

                while (!headOldIsFound)
                {
                    var result = await PostApi.Get(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            type, countPerTime, offset)
                        .ConfigureAwait(false);

                    if (result?.IsError != false)
                        continue;

                    result.Data ??= new List<PostSchema>();

                    if (result.Data.Count == 0)
                    {
                        await Dispatcher.Invoke(() =>
                        {
                            return UpdatePosts();
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    if (isFirstRequest)
                    {
                        headNewId = result.Data[0].Id;
                        isFirstRequest = false;
                    }

                    foreach (var post in result.Data)
                    {
                        if (post.Id == headOldId)
                        {
                            headOldIsFound = true;

                            break;
                        }

                        ++countNew;
                    }

                    if (countNew >= 500)
                    {
                        await Dispatcher.Invoke(() =>
                        {
                            return UpdatePosts();
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    offset += countPerTime;
                }

                Dispatcher.Invoke(() =>
                {
                    ViewModel.LastNewHeadPostId = headNewId;
                });

                return countNew;
            }).ConfigureAwait(true);
        }



        private async Task<int> GetAllPostsCount(
            PostType type, int countPerTime, int offset)
        {
            if (!(type == PostType.My || type == PostType.Favorite))
                return 0;

            return await Task.Run(async () =>
            {
                var countNew = 0;

                while (true)
                {
                    var result = await PostApi.Get(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            type, countPerTime, offset)
                        .ConfigureAwait(false);

                    if (result?.IsError != false)
                        continue;

                    result.Data ??= new List<PostSchema>();

                    if (result.Data.Count == 0)
                        break;

                    countNew += result.Data.Count;
                    offset += countPerTime;
                }

                return countNew;
            }).ConfigureAwait(true);
        }



        public Task ShowLoadingGrid()
        {
            LoadingIndicator.IsActive = true;
            LoadingGrid.Opacity = 1.0;
            LoadingGrid.IsHitTestVisible = true;
            LoadingGrid.Visibility = Visibility.Visible;

            return Task.CompletedTask;
        }

        public Task HideLoadingGrid()
        {
            LoadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (var i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LoadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    LoadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }

        public void ShowLoadingMoreGrid()
        {
            LoadingMoreIndicator.IsActive = true;
            LoadingMoreGrid.IsHitTestVisible = true;

            LoadingMoreShowAnimation.Begin();
        }

        public void HideLoadingMoreGrid()
        {
            LoadingMoreIndicator.IsActive = false;
            LoadingMoreGrid.IsHitTestVisible = false;

            LoadingMoreHideAnimation.Begin();
        }



        protected override void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            ViewModel.OnPostScrollEnd = new AsyncBasicCommand(
                async _ =>
                {
                    if (PostsScrollViewer.HorizontalOffset == 0)
                        return;

                    await LoadMorePosts()
                        .ConfigureAwait(true);
                });

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            _autoUpdateCountTimer.Start();
        }

        protected override void OnExit(object sender,
            RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            _autoUpdateCountTimer.Stop();

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }



        private void OnLocalizationUpdated(object sender,
            LocalizationEventArgs e)
        {
            UpdatePostsTypes();
        }

        private async void OnCurrentUserChanged(object sender,
            UserChangedEventArgs e)
        {
            if (e.NewUser.Id == -1)
                return;

            _autoUpdateCountTimer.Stop();

            PostsTypesComboBox.IsEnabled = false;
            RefreshPostsButton.IsEnabled = false;

            await ShowLoadingGrid()
                .ConfigureAwait(true);

            PostsScrollViewer.IsEnabled = false;

            try
            {
                var postType =
                    ((KeyValuePair<PostType, string>)PostsTypesComboBox.SelectedItem)
                    .Key;

                switch (postType)
                {
                    case PostType.Popular:
                    case PostType.New:
                        if (PostsWrapPanel.Children.Count == 0)
                            break;

                        var tasks = new List<Task>(
                            PostsWrapPanel.Children.Count);

                        foreach (var post in PostsWrapPanel.Children)
                        {
                            if (!(post is Post postWidget))
                                continue;

                            tasks.Add(postWidget
                                .UpdatePost());

                            if (tasks.Count % 5 == 0)
                            {
                                await Task.Delay(
                                        TimeSpan.FromSeconds(0.5))
                                    .ConfigureAwait(true);
                            }
                        }

                        await Task.WhenAll(tasks)
                            .ConfigureAwait(true);

                        break;
                    case PostType.My:
                    case PostType.Favorite:
                        await UpdatePosts()
                            .ConfigureAwait(true);

                        break;
                    default:
                        break;
                }
            }
            finally
            {
                PostsScrollViewer.IsEnabled = true;

                await HideLoadingGrid()
                    .ConfigureAwait(true);

                RefreshPostsButton.IsEnabled = true;
                PostsTypesComboBox.IsEnabled = true;

                _autoUpdateCountTimer.Start();
            }
        }



        private void Post_Click(object sender,
            RoutedEventArgs e)
        {
            if (!(sender is Post post))
                return;

            NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayViewModel
            {
                SourcePost = post,
                CurrentPostData = post.CurrentPostData
            });
        }

        private async void Post_Delete(object sender,
            RoutedEventArgs e)
        {
            _autoUpdateCountTimer.Stop();

            if (!(sender is Post post))
            {
                _autoUpdateCountTimer.Start();

                return;
            }

            if (ViewModel.LastNewHeadPostId == post.CurrentPostData.Id)
            {
                await UpdatePosts()
                    .ConfigureAwait(true);

                _autoUpdateCountTimer.Start();

                return;
            }

            PostsWrapPanel.Children.Remove(post);

            --ViewModel.Offset;

            if (PostsWrapPanel.Children.Count == 0)
                IsEmpty = true;

            _autoUpdateCountTimer.Start();
        }



        private async void NewPostsCountButton_Click(object sender,
            RoutedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private async void PostsTypesComboBox_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);

            SettingsManager.AppSettings.PostsTypeEnum =
                ((KeyValuePair<PostType, string>)PostsTypesComboBox.SelectedItem)
                .Key;

            SettingsManager.AppSettings.Save();
        }

        private async void RefreshPostsButton_Click(object sender,
            RoutedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private void CreatePostButton_Click(object sender,
            RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<SubmitPostPage>();
        }

        private void PostsScrollViewer_ScrollChanged(object sender,
            ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.HorizontalOffset;
        }



        private async void AutoUpdateCountTimer_Tick(object sender,
            ElapsedEventArgs e)
        {
            if (!_autoUpdateCountTimer.Enabled)
                return;

            if (State != ControlStateType.Loaded)
                return;

            _autoUpdateCountTimer.Stop();

            var newPostsCount = await GetNewPostsCount()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.Offset += newPostsCount;
                ViewModel.NewPostsCount += newPostsCount;
            });

            _autoUpdateCountTimer.Start();
        }
    }
}

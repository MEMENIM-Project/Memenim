using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Memenim.Layouts;
using Memenim.Layouts.NavigationBar;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using RIS;
using RIS.Collections.Chunked;
using RIS.Collections.Extensions;

namespace Memenim.Navigation
{
    public sealed partial class NavigationController : UserControl
    {
        private static readonly object InstanceSyncRoot = new object();
        private static volatile NavigationController _instance;
        public static NavigationController Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceSyncRoot)
                    {
                        if (_instance == null)
                            _instance = new NavigationController();
                    }
                }

                return _instance;
            }
        }



        private readonly Dictionary<Type, NavBarLayoutType> _navBarPagesLayouts;
        private readonly ChunkedArrayL<NavigationHistoryNode> _navigationHistory;

        private PageContentType _currentPageContentType;
        private bool _currentPageSkipWhenGoBack;



        public bool IsCreated { get; private set; }



        private NavigationController()
        {
            Initialized += OnCreated;
            Initialized += OnInitialized;
            Loaded += OnEnter;
            Unloaded += OnExit;

            IsCreated = false;

            _navBarPagesLayouts = new Dictionary<Type, NavBarLayoutType>();
            _navigationHistory = new ChunkedArrayL<NavigationHistoryNode>();

            _currentPageContentType = PageContentType.Page;
            _currentPageSkipWhenGoBack = false;

            InitializeComponent();
            DataContext = this;
        }



        private async Task LoadNavBarLayouts()
        {
            foreach (var layoutType in (NavBarLayoutType[])Enum.GetValues(typeof(NavBarLayoutType)))
            {
                var dictionary = await LayoutsManager
                    .GetLayout(NavBar, layoutType.ToString())
                    .ConfigureAwait(true);

                if (dictionary == null)
                    continue;

                foreach (var pageName in (string[])dictionary["TriggeredOnPages"])
                {
                    var pageType = Type.GetType(
                        $"Memenim.Pages.{pageName}");

                    if (pageType == null)
                        continue;

                    if (!_navBarPagesLayouts.ContainsKey(pageType))
                        _navBarPagesLayouts.Add(pageType, layoutType);
                }
            }
        }

        private NavBarLayoutType? GetNavBarLayoutType(
            PageContent page)
        {
            var pageType = page.GetType();

            if (!_navBarPagesLayouts.ContainsKey(pageType))
                return null;

            return _navBarPagesLayouts[pageType];
        }

        private async Task SwitchNavBarLayout(
            NavBarLayoutType type)
        {
            if (type == NavBarLayoutType.NavBarNone)
            {
                MainWindow.Instance.DeactivateOpenLink();

                RootLayout.DisplayMode = SplitViewDisplayMode.Inline;

                MainWindow.Instance.SettingsFlyout.Hide();

                await NavBar.SwitchLayout(type)
                    .ConfigureAwait(true);
            }
            else
            {
                await NavBar.SwitchLayout(type)
                    .ConfigureAwait(true);

                RootLayout.DisplayMode = SplitViewDisplayMode.CompactInline;

                MainWindow.Instance.ActivateOpenLink();
            }
        }

        private async Task SwitchNavBarLayout(
            PageContent page)
        {
            var layoutType = GetNavBarLayoutType(page);

            if (!layoutType.HasValue)
                return;

            await SwitchNavBarLayout(layoutType.Value)
                .ConfigureAwait(true);
        }



        private ContentControl GetCurrentContentControl()
        {
            return _currentPageContentType switch
            {
                PageContentType.Page => PageContent,
                PageContentType.Overlay => OverlayContent,
                _ => throw new ArgumentException(
                    "Invalid page content type",
                    nameof(_currentPageContentType))
            };
        }

        private PageContent GetCurrentContent()
        {
            return GetCurrentContentControl()
                .Content as PageContent;
        }


        private void ActivateEnterContentEvent()
        {
            var content = GetCurrentContent();

            if (content == null)
                return;

            content.IsOnEnterActive = true;
        }

        private void ActivateExitContentEvent()
        {
            var content = GetCurrentContent();

            if (content == null)
                return;

            content.IsOnExitActive = true;
        }


        private void DeactivateEnterContentEvent()
        {
            var content = GetCurrentContent();

            if (content == null)
                return;

            content.IsOnEnterActive = false;
        }

        private void DeactivateExitContentEvent()
        {
            var content = GetCurrentContent();

            if (content == null)
                return;

            content.IsOnExitActive = false;
        }



        private bool IsOpenOverlay()
        {
            return _currentPageContentType == PageContentType.Overlay;
        }

        private void ShowOverlay()
        {
            OverlayLayout.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            OverlayLayout.Visibility = Visibility.Collapsed;
        }

        private void ClearOverlay()
        {
            OverlayContent.Content = null;
        }


        private PageContent GetCurrentOverlay()
        {
            return OverlayContent
                .Content as PageContent;
        }



        private bool IsOpenPage()
        {
            return _currentPageContentType == PageContentType.Page;
        }


        private PageContent GetCurrentPage()
        {
            return PageContent
                .Content as PageContent;
        }



        private void SetPage(
            PageContent page,
            PageViewModel dataContext = null,
            bool skipWhenGoBack = false)
        {
            if (!IsOpenOverlay()
                && ReferenceEquals(PageContent.Content, page)
                && (dataContext == null
                    || ReferenceEquals((PageContent.Content as PageContent)?.DataContext, dataContext)))
            {
                return;
            }

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            SaveContentToHistory();

            if (dataContext != null)
            {
                if (dataContext.PageType != page.GetType())
                {
                    var exception =
                        new ArgumentException($"The {nameof(dataContext)} page type must match the {nameof(page)} type", nameof(dataContext));
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                page.DataContext = dataContext;
            }

            page.IsOnEnterActive = true;
            page.IsOnExitActive = true;

            PageContent.Content = page;
            _currentPageContentType = PageContentType.Page;
            _currentPageSkipWhenGoBack = skipWhenGoBack;

            SwitchNavBarLayout(page).Wait();

            HideOverlay();
            ClearOverlay();

            if (!_navigationHistory.IsEmpty())
            {
                ref var node = ref _navigationHistory
                    .PeekRef();

                if (!ReferenceEquals(GetCurrentContent(), node.Content))
                {
                    node.Content.IsOnEnterActive = false;
                    node.Content.IsOnExitActive = true;
                }
            }
        }

        private void SetOverlay(
            PageContent page,
            PageViewModel dataContext = null,
            bool skipWhenGoBack = false)
        {
            if (ReferenceEquals(OverlayContent.Content, page)
                && (dataContext == null
                    || ReferenceEquals((OverlayContent.Content as PageContent)?.DataContext, dataContext)))
            {
                return;
            }

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            SaveContentToHistory();

            if (dataContext != null)
            {
                if (dataContext.PageType != page.GetType())
                {
                    var exception =
                        new ArgumentException($"The {nameof(dataContext)} page type must match the {nameof(page)} type", nameof(dataContext));
                    Events.OnError(this, new RErrorEventArgs(exception, exception.Message));
                    throw exception;
                }

                page.DataContext = dataContext;
            }

            page.IsOnEnterActive = true;
            page.IsOnExitActive = true;

            OverlayContent.Content = page;
            _currentPageContentType = PageContentType.Overlay;
            _currentPageSkipWhenGoBack = skipWhenGoBack;

            SwitchNavBarLayout(page).Wait();

            ShowOverlay();

            if (!_navigationHistory.IsEmpty())
            {
                ref var node = ref _navigationHistory
                    .PeekRef();

                if (!ReferenceEquals(GetCurrentContent(), node.Content))
                {
                    node.Content.IsOnEnterActive = false;
                    node.Content.IsOnExitActive = true;
                }
            }
        }



        private void LoadContentFromHistory(
            bool autoSkip = false)
        {
            if (_navigationHistory.Length == 0)
                return;

            NavigationHistoryNode node;

            do
            {
                node = _navigationHistory.Pop();
            } while (autoSkip
                     && node.SkipWhenGoBack
                     && _navigationHistory.Length != 0);

            var oldContent = GetCurrentContent();

            switch (node.Type)
            {
                case PageContentType.Page:
                {
                    node.Content.IsOnEnterActive = true;
                    node.Content.IsOnExitActive = true;

                    node.Content.DataContext = node.DataContext;
                    PageContent.Content = node.Content;

                    SwitchNavBarLayout(node.Content).Wait();

                    HideOverlay();
                    ClearOverlay();

                    break;
                }
                case PageContentType.Overlay:
                {
                    if (node.SubContent != null)
                    {
                        node.SubContent.IsOnEnterActive = true;
                        node.SubContent.IsOnExitActive = true;

                        node.SubContent.DataContext = node.SubDataContext;
                        PageContent.Content = node.SubContent;
                    }

                    node.Content.IsOnEnterActive = true;
                    node.Content.IsOnExitActive = true;

                    node.Content.DataContext = node.DataContext;
                    OverlayContent.Content = node.Content;

                    SwitchNavBarLayout(node.Content).Wait();

                    ShowOverlay();

                    break;
                }
                default:
                    throw new ArgumentException(
                        "Invalid page content type for history node",
                        nameof(node.Type));
            }

            _currentPageContentType = node.Type;

            if (!ReferenceEquals(GetCurrentContent(), oldContent))
            {
                oldContent.IsOnEnterActive = false;
                oldContent.IsOnExitActive = true;
            }
        }

        private void SaveContentToHistory()
        {
            var contentControl = GetCurrentContentControl();

            if (contentControl.Content == null)
                return;

            var layoutType = GetNavBarLayoutType(
                (PageContent)contentControl.Content);

            if (!layoutType.HasValue
                || layoutType.Value == NavBarLayoutType.NavBarNone)
            {
                return;
            }

            _navigationHistory.Push(new NavigationHistoryNode
            {
                Content = contentControl.Content as PageContent,
                SubContent = _currentPageContentType switch
                {
                    PageContentType.Page => null,
                    PageContentType.Overlay => PageContent.Content as PageContent,
                    _ => throw new ArgumentException(
                        "Invalid page content type for current page",
                        nameof(_currentPageContentType))
                },
                DataContext = (contentControl.Content as PageContent)?.DataContext as PageViewModel,
                SubDataContext = _currentPageContentType switch
                {
                    PageContentType.Page => null,
                    PageContentType.Overlay => (PageContent.Content as PageContent)?.DataContext as PageViewModel,
                    _ => throw new ArgumentException(
                        "Invalid page content type for current page",
                        nameof(_currentPageContentType))
                },
                Type = _currentPageContentType,
                SkipWhenGoBack = _currentPageSkipWhenGoBack
            });
        }



        public void RequestPage<T>(
            PageViewModel dataContext = null,
            bool skipWhenGoBack = false)
            where T : PageContent
        {
            var page = PageStorage.GetPage<T>();

            SetPage(page, dataContext,
                skipWhenGoBack);
        }
        public void RequestPage(Type type,
            PageViewModel dataContext = null,
            bool skipWhenGoBack = false)
        {
            var page = PageStorage.GetPage(type);

            SetPage(page, dataContext,
                skipWhenGoBack);
        }

        public void RequestOverlay<T>(
            PageViewModel dataContext = null,
            bool skipWhenGoBack = false)
            where T : PageContent
        {
            var page = PageStorage.GetPage<T>();

            SetOverlay(page, dataContext,
                skipWhenGoBack);
        }
        public void RequestOverlay(Type type,
            PageViewModel dataContext = null,
            bool skipWhenGoBack = false)
        {
            var page = PageStorage.GetPage(type);

            SetOverlay(page, dataContext,
                skipWhenGoBack);
        }


        public bool IsCurrentContent<T>()
            where T : PageContent
        {
            return GetCurrentContent()?
                .GetType() == typeof(T);
        }
        public bool IsCurrentContent(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException("The page class must be derived from the PageContent", nameof(type));
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return GetCurrentContent()?
                .GetType() == type;
        }

        public bool IsCurrentPage<T>()
            where T : PageContent
        {
            return GetCurrentPage()?
                .GetType() == typeof(T);
        }
        public bool IsCurrentPage(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException("The page class must be derived from the PageContent", nameof(type));
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return GetCurrentPage()?
                .GetType() == type;
        }

        public bool IsCurrentOverlay<T>()
            where T : PageContent
        {
            return GetCurrentOverlay()?
                .GetType() == typeof(T);
        }
        public bool IsCurrentOverlay(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException("The page class must be derived from the PageContent", nameof(type));
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            return GetCurrentOverlay()?
                .GetType() == type;
        }


        public void GoBack(
            bool autoSkip = false)
        {
            LoadContentFromHistory(autoSkip);
        }
        public void GoBack(int count,
            bool autoSkip = false)
        {
            if (count < 0)
                count = 0;

            for (var i = 0; i < count; ++i)
            {
                GoBack(autoSkip);
            }
        }



        public void ActivateNavigation()
        {
            NavBar.IsEnabled = true;
        }

        public void DeactivateNavigation()
        {
            NavBar.IsEnabled = false;
        }



        public bool IsEmptyHistory()
        {
            return _navigationHistory.Length == 0;
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }



        private async void OnCreated(object sender,
            EventArgs e)
        {
            if (IsCreated)
                return;

            Initialized -= OnCreated;

            IsCreated = true;

            await LoadNavBarLayouts()
                .ConfigureAwait(true);

            HideOverlay();
        }

        private void OnInitialized(object sender,
            EventArgs e)
        {

        }

        private void OnEnter(object sender,
            RoutedEventArgs e)
        {

        }

        private void OnExit(object sender,
            RoutedEventArgs e)
        {

        }



        private void OverlayBackground_Click(object sender,
            RoutedEventArgs e)
        {
            if (OverlayContent.Content != null)
            {
                var layoutType = GetNavBarLayoutType(
                    (PageContent)OverlayContent.Content);

                if (!layoutType.HasValue
                    || layoutType.Value == NavBarLayoutType.NavBarNone
                    || layoutType.Value == NavBarLayoutType.NavBarBackOnly)
                {
                    GoBack();

                    return;
                }
            }

            var page = PageContent.Content as PageContent;
            var dataContext = page?.DataContext as PageViewModel;

            SetPage(page, dataContext);
        }
    }
}

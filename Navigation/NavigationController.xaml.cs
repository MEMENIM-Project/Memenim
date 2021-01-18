using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.TabLayouts;
using Memenim.TabLayouts.NavigationBar;
using RIS;

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

        private readonly Dictionary<Type, NavBarLayoutType> _navBarPagesLayouts =
            new Dictionary<Type, NavBarLayoutType>();
        private readonly Stack<NavigationHistoryNode> _navigationHistory =
            new Stack<NavigationHistoryNode>();
        private PageContentType _currentPageContentType = PageContentType.Page;

        private NavigationController()
        {
            InitializeComponent();
            DataContext = this;

            LoadNavBarLayouts().Wait();

            HideOverlay();
        }

        private async Task LoadNavBarLayouts()
        {
            foreach (var layoutType in (NavBarLayoutType[])Enum.GetValues(typeof(NavBarLayoutType)))
            {
                ResourceDictionary dictionary = await TabLayoutsManager.GetLayout(NavBar, layoutType.ToString())
                    .ConfigureAwait(true);

                if (dictionary == null)
                    continue;

                foreach (var pageName in (string[])dictionary["TriggeredOnPages"])
                {
                    Type pageType = Type.GetType($"Memenim.Pages.{pageName}");

                    if (pageType == null)
                        continue;

                    if (!_navBarPagesLayouts.ContainsKey(pageType))
                        _navBarPagesLayouts.Add(pageType, layoutType);
                }
            }
        }

        private NavBarLayoutType? GetNavBarLayoutType(PageContent page)
        {
            Type pageType = page.GetType();

            if (!_navBarPagesLayouts.ContainsKey(pageType))
                return null;

            return _navBarPagesLayouts[pageType];
        }

        private async Task SwitchNavBarLayout(NavBarLayoutType type)
        {
            if (type == NavBarLayoutType.NavBarNone)
            {
                RootLayout.DisplayMode = SplitViewDisplayMode.Inline;

                await NavBar.SwitchLayout(type)
                    .ConfigureAwait(true);
            }
            else
            {
                await NavBar.SwitchLayout(type)
                    .ConfigureAwait(true);

                RootLayout.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
        }

        private async Task SwitchNavBarLayout(PageContent page)
        {
            var layoutType = GetNavBarLayoutType(page);

            if (!layoutType.HasValue)
                return;

            await SwitchNavBarLayout(layoutType.Value)
                .ConfigureAwait(true);
        }

        private void SetPage(PageContent page, PageViewModel dataContext = null)
        {
            if (!OverlayIsOpen()
                && ReferenceEquals(PageContent.Content, page)
                && (dataContext == null
                    || ReferenceEquals((PageContent.Content as PageContent)?.DataContext, dataContext)))
            {
                return;
            }

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            IsContentEventsActive(false);

            SaveContentToHistory();

            if (dataContext != null)
            {
                if (dataContext.PageType != page.GetType())
                {
                    var exception =
                        new ArgumentException($"The {nameof(dataContext)} page type must match the {nameof(page)} type", nameof(dataContext));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                page.DataContext = dataContext;
            }

            PageContent.Content = page;
            _currentPageContentType = PageContentType.Page;

            SwitchNavBarLayout(page).Wait();

            HideOverlay();
            ClearOverlay();

            IsContentEventsActive(true);

            //UpdateLayout();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
        }

        private void SetOverlay(PageContent page, PageViewModel dataContext = null)
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

            IsContentEventsActive(false);

            SaveContentToHistory();

            if (dataContext != null)
            {
                if (dataContext.PageType != page.GetType())
                {
                    var exception =
                        new ArgumentException($"The {nameof(dataContext)} page type must match the {nameof(page)} type", nameof(dataContext));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                page.DataContext = dataContext;
            }

            OverlayContent.Content = page;
            _currentPageContentType = PageContentType.Overlay;

            SwitchNavBarLayout(page).Wait();

            ShowOverlay();

            IsContentEventsActive(true);

            //UpdateLayout();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
        }

        private void LoadContentFromHistory()
        {
            if (_navigationHistory.Count == 0)
                return;

            IsContentEventsActive(false);

            var node = _navigationHistory.Pop();

            switch (node.Type)
            {
                case PageContentType.Page:
                {
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
                        node.SubContent.DataContext = node.SubDataContext;
                        PageContent.Content = node.SubContent;
                    }

                    node.Content.DataContext = node.DataContext;
                    OverlayContent.Content = node.Content;

                    SwitchNavBarLayout(node.Content).Wait();

                    ShowOverlay();

                    break;
                }
            }

            _currentPageContentType = node.Type;
        }

        private void SaveContentToHistory()
        {
            IsContentEventsActive(false);

            var contentControl = _currentPageContentType switch
            {
                PageContentType.Page => PageContent,
                PageContentType.Overlay => OverlayContent
            };

            if (contentControl.Content != null)
            {
                var layoutType = GetNavBarLayoutType((PageContent)contentControl.Content);

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
                        PageContentType.Overlay => PageContent.Content as PageContent
                    },
                    DataContext = (contentControl.Content as PageContent)?.DataContext as PageViewModel,
                    SubDataContext = _currentPageContentType switch
                    {
                        PageContentType.Page => null,
                        PageContentType.Overlay => (PageContent.Content as PageContent)?.DataContext as PageViewModel
                    },
                    Type = _currentPageContentType
                });
            }
        }

        private void IsContentEventsActive(bool isActive)
        {
            IsContentEventsActive(_currentPageContentType, isActive);
        }
        private void IsContentEventsActive(PageContentType type, bool isActive)
        {
            var contentControl = type switch
            {
                PageContentType.Page => PageContent,
                PageContentType.Overlay => OverlayContent
            };

            PageContent content = contentControl.Content as PageContent;

            if (content == null)
                return;

            content.IsOnEnterActive = isActive;
            content.IsOnExitActive = isActive;
        }

        private bool OverlayIsOpen()
        {
            return OverlayLayout.Visibility == Visibility.Visible;
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

        public void RequestPage<T>(PageViewModel dataContext = null)
            where T : PageContent
        {
            PageContent page = PageStorage.GetPage<T>();

            SetPage(page, dataContext);
        }
        public void RequestPage(Type type, PageViewModel dataContext = null)
        {
            PageContent page = PageStorage.GetPage(type);

            SetPage(page, dataContext);
        }

        public void RequestOverlay<T>(PageViewModel dataContext = null)
            where T : PageContent
        {
            PageContent page = PageStorage.GetPage<T>();

            SetOverlay(page, dataContext);
        }
        public void RequestOverlay(Type type, PageViewModel dataContext = null)
        {
            PageContent page = PageStorage.GetPage(type);

            SetOverlay(page, dataContext);
        }

        public void GoBack()
        {
            LoadContentFromHistory();
        }

        public bool HistoryIsEmpty()
        {
            return _navigationHistory.Count == 0;
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        public void LockNavigation(bool state)
        {
            if (state)
            {
                NavBar.IsEnabled = false;

                return;
            }

            NavBar.IsEnabled = true;
        }

        private void OverlayBackground_Click(object sender, RoutedEventArgs e)
        {
            IsContentEventsActive(false);

            if (OverlayContent.Content != null)
            {
                var layoutType = GetNavBarLayoutType((PageContent)OverlayContent.Content);

                if (!layoutType.HasValue
                    || layoutType.Value == NavBarLayoutType.NavBarNone
                    || layoutType.Value == NavBarLayoutType.NavBarBackOnly)
                {
                    GoBack();
                    IsContentEventsActive(true);

                    return;
                }
                else
                {
                    _navigationHistory.Push(new NavigationHistoryNode
                    {
                        Content = OverlayContent.Content as PageContent,
                        SubContent = PageContent.Content as PageContent,
                        DataContext = (OverlayContent.Content as PageContent)?.DataContext as PageViewModel,
                        SubDataContext = (PageContent.Content as PageContent)?.DataContext as PageViewModel,
                        Type = PageContentType.Overlay
                    });
                }
            }

            _currentPageContentType = PageContentType.Page;

            HideOverlay();
            ClearOverlay();

            IsContentEventsActive(true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Memenim.Pages;
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

        private NavBarLayoutType GetNavBarLayoutType(PageContent page)
        {
            Type pageType = page.GetType();

            if (!_navBarPagesLayouts.ContainsKey(pageType))
                return NavBarLayoutType.NavBarNone;

            return _navBarPagesLayouts[pageType];
        }

        private async Task SwitchNavBarLayout(NavBarLayoutType type)
        {
            if (type == NavBarLayoutType.NavBarNone)
            {
                RootLayout.DisplayMode = SplitViewDisplayMode.Inline;

                ClearHistory();

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
            Type pageType = page.GetType();

            if (!_navBarPagesLayouts.ContainsKey(pageType))
                return;

            await SwitchNavBarLayout(_navBarPagesLayouts[pageType])
                .ConfigureAwait(true);
        }

        private void SetPage(PageContent page, PageContent dataContext = null)
        {
            if (ReferenceEquals(PageContent.Content, page))
                return;

            if (dataContext != null)
            {
                if (dataContext.GetType() != page.GetType())
                {
                    var exception =
                        new ArgumentException($"The {nameof(dataContext)} type must match the {nameof(page)} type", nameof(dataContext));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                page = dataContext;
            }

            if (ReferenceEquals(PageContent.Content, page))
                return;

            SaveContentToHistory();

            PageContent.Content = page;
            _currentPageContentType = PageContentType.Page;

            SwitchNavBarLayout(page).Wait();

            HideOverlay();
        }

        private void SetOverlay(PageContent page, PageContent dataContext = null)
        {
            if (ReferenceEquals(PageContent.Content, page))
                return;

            if (dataContext != null)
            {
                if (dataContext.GetType() != page.GetType())
                {
                    var exception =
                        new ArgumentException($"The {nameof(dataContext)} type must match the {nameof(page)} type", nameof(dataContext));
                    Events.OnError(this, new RErrorEventArgs(exception.Message, exception.StackTrace));
                    throw exception;
                }

                page = dataContext;
            }

            if (ReferenceEquals(PageContent.Content, page))
                return;

            SaveContentToHistory();

            OverlayContent.Content = page;
            _currentPageContentType = PageContentType.Overlay;

            SwitchNavBarLayout(page).Wait();

            ShowOverlay();
        }

        private void LoadContentFromHistory()
        {
            if (_navigationHistory.Count == 0)
                return;

            var node = _navigationHistory.Pop();

            switch (node.Type)
            {
                case PageContentType.Page:
                    {
                        PageContent.Content = node.Content;

                        SwitchNavBarLayout(node.Content).Wait();

                        HideOverlay();

                        break;
                    }
                case PageContentType.Overlay:
                    {
                        if (node.ContentContext != null)
                            PageContent.Content = node.ContentContext;

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
            var contentControl = _currentPageContentType switch
            {
                PageContentType.Page => PageContent,
                PageContentType.Overlay => OverlayContent
            };

            if (contentControl.Content != null)
            {
                if (GetNavBarLayoutType((PageContent)contentControl.Content) == NavBarLayoutType.NavBarNone)
                    return;

                _navigationHistory.Push(new NavigationHistoryNode
                {
                    Content = contentControl.Content as PageContent,
                    ContentContext = _currentPageContentType switch
                    {
                        PageContentType.Page => null,
                        PageContentType.Overlay => PageContent.Content as PageContent
                    },
                    Type = _currentPageContentType
                });
            }
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

        public void RequestPage<T>(T dataContext = null)
            where T : PageContent
        {
            PageContent page = PageStorage.GetPage<T>();

            SetPage(page, dataContext);
        }
        public void RequestPage(Type type, PageContent dataContext = null)
        {
            PageContent page = PageStorage.GetPage(type);

            SetPage(page, dataContext);
        }

        public void RequestOverlay<T>(T dataContext = null)
            where T : PageContent
        {
            PageContent page = PageStorage.GetPage<T>();

            SetOverlay(page, dataContext);
        }
        public void RequestOverlay(Type type, PageContent dataContext = null)
        {
            PageContent page = PageStorage.GetPage(type);

            SetOverlay(page, dataContext);
        }

        public void GoBack()
        {
            LoadContentFromHistory();
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        private void OverlayBackground_Click(object sender, RoutedEventArgs e)
        {
            if (OverlayContent.Content != null)
            {
                _navigationHistory.Push(new NavigationHistoryNode
                {
                    Content = OverlayContent.Content as PageContent,
                    ContentContext = PageContent.Content as PageContent,
                    Type = PageContentType.Overlay
                });
            }

            _currentPageContentType = PageContentType.Page;

            HideOverlay();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Memenim.Pages;
using RIS;

namespace Memenim.Navigation
{
    public sealed partial class NavigationController : UserControl
    {
        private static NavigationController _instance;
        public static NavigationController Instance
        {
            get
            {
                return _instance ??= new NavigationController();
            }
        }

        private readonly Stack<NavigationHistoryNode> _navigationHistory = new Stack<NavigationHistoryNode>();
        private PageContentType _currentPageContentType = PageContentType.Page;

        private NavigationController()
        {
            InitializeComponent();
            DataContext = this;

            HideOverlay();
        }

        private void SetPage(PageContent page, PageContent dataContext = null)
        {
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

            SaveContentToHistory();

            PageContent.Content = page;
            _currentPageContentType = PageContentType.Page;

            HideOverlay();
        }

        private void SetOverlay(PageContent page, PageContent dataContext = null)
        {
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

            SaveContentToHistory();

            OverlayContent.Content = page;
            _currentPageContentType = PageContentType.Overlay;

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
                    HideOverlay();

                    break;
                }
                case PageContentType.Overlay:
                {
                    if (node.ContentContext != null)
                        PageContent.Content = node.ContentContext;

                    OverlayContent.Content = node.Content;
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

        private void LoadTabLayouts()
        {

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

        private void OverlayBackgroundClick(object sender, RoutedEventArgs e)
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

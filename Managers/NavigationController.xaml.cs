using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Memenim.Pages;

namespace Memenim.Managers
{
    /// <summary>
    /// Interaction logic for NavigationController.xaml
    /// </summary>
    public partial class NavigationController : UserControl
    {
        public static NavigationController Instance
        {
            get 
            {
                if (m_Instance == null)
                    m_Instance = new NavigationController();
                return m_Instance; 
            }
        }

        private static NavigationController m_Instance;

        private Queue<PageContent> m_NavigationHistory = new Queue<PageContent>();

        public NavigationController()
        {
            InitializeComponent();
            HideOverlay();
        }

        private void OverlayBackgroundClick(object sender, RoutedEventArgs e)
        {
            HideOverlay();
        }

        public void RequestOverlay<T>(object dataContext = null) where T : Pages.PageContent
        {
            Pages.PageContent pg = PageStorage.GetPage<T>();
            pg.DataContext = dataContext;
            OverlayContent.Content = pg;
            OverlayLayout.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            OverlayContent.Content = null;
            OverlayLayout.Visibility = Visibility.Collapsed;
        }

        private void LoadTabLayouts()
        {

        }

        public void RequestPage<T>(object dataContext = null) where T : PageContent
        {
            PageContent pg = PageStorage.GetPage<T>();
            SetPageInternal(pg, dataContext);
        }

        public void RequestPage(Type pageType, object dataContext = null)
        {
            PageContent pg = PageStorage.GetPage(pageType);
            SetPageInternal(pg, dataContext);
        }

        private void SetPageInternal(PageContent page, object dataContext = null)
        {
            if (OverlayLayout.Visibility == Visibility.Visible)
            {
                HideOverlay();
            }
            page.DataContext = dataContext;
            if (PageContent.Content != null)
            {
                m_NavigationHistory.Enqueue(PageContent.Content as PageContent);
            }
            PageContent.Content = page;
        }

        public void GoBack()
        {
            PageContent.Content = m_NavigationHistory.Dequeue();
        }
    }
}

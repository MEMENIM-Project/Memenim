using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace Memenim.Pages
{
    public static class PageNavigationManager
    {
        public static MetroContentControl PageContentControl { get; set; }
        public static TransitioningContentControl SubPageContentControl { get; set; }
        public static TransitioningContentControl OverlayContentControl { get; set; }

        static PageNavigationManager()
        {
            PageContentControl = new MetroContentControl();
            SubPageContentControl = new TransitioningContentControl();
            OverlayContentControl = new TransitioningContentControl();
        }

        public static void GoBack()
        {
            UserControl backPage = GeneralBlackboard.TryGetValue<UserControl>(BlackBoardValues.EBackPage);

            if (backPage != null)
                SwitchToSubPage(backPage);
        }

        public static void OpenOverlay(object overlay)
        {
            OverlayContentControl.Content = overlay;
        }

        public static void CloseOverlay()
        {
            OverlayContentControl.Content = null;
        }

        public static void SwitchToPage(object page)
        {
            PageContentControl.Content = page;
        }

        public static void SwitchToSubPage(object subPage)
        {
            SubPageContentControl.Content = subPage;
        }
    }
}

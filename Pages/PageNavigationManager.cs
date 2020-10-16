using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace Memenim.Pages
{
    static class PageNavigationManager
    {
        static public MetroContentControl PageContentControl
        {
            set { m_PageContentControl = value; }
        }

        static public TransitioningContentControl SubpageContentControl
        {
            set { m_SubPageContentControl = value; }
        }

        static public TransitioningContentControl OverlayContentControl
        {
            set { m_OverlayControlControl = value; }
        }

        static private MetroContentControl m_PageContentControl;
        static private TransitioningContentControl m_SubPageContentControl;
        static private TransitioningContentControl m_OverlayControlControl;

        public static void SwitchToPage(object page)
        {
            m_PageContentControl.Content = page;
        }

        public static void SwitchToSubpage(object subpage)
        {
            m_SubPageContentControl.Content = subpage;
        }

        public static void GoBack()
        {
            UserControl backPage = GeneralBlackboard.TryGetValue<UserControl>(BlackBoardValues.EBackPage);
            if (backPage != null)
            {
                SwitchToSubpage(backPage);
            }
        }

        public static void OpenOverlay(object overlay)
        {
            m_OverlayControlControl.Content = overlay;
        }

        public static void CloseOverlay()
        {
            m_OverlayControlControl.Content = null;
        }
    }
}

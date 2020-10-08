using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AnonymDesktopClient.Pages
{
    static class PageNavigationManager
    {
        static public MetroContentControl PageContentControl
        {
            set { m_PageContentControl = value; }
        }

        static public MetroContentControl SubpageContentControl
        {
            set { m_SubPageContentControl = value; }
        }

        static public MetroContentControl OverlayContentControl
        {
            set { m_OverlayControlControl = value; }
        }

        static private MetroContentControl m_PageContentControl;
        static private MetroContentControl m_SubPageContentControl;
        static private MetroContentControl m_OverlayControlControl;

        public static void SwitchToPage(object page)
        {
            m_PageContentControl.Content = page;
        }

        public static void SwitchToSubpage(object subpage)
        {
            m_SubPageContentControl.Content = subpage;
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

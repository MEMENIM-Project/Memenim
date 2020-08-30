using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Controls;

namespace AnonymDesktopClient.Pages
{
    static class PageSwitcher
    {
        static public MetroContentControl PageContentControl
        {
            set { m_PageContentControl = value; }
        }

        static public ContentControl SubpageContentControl
        {
            set { m_SubPageContentControl = value; }
        }

        static private MetroContentControl m_PageContentControl;
        static private ContentControl m_SubPageContentControl;

        public static void SwitchToPage(object page)
        {
            m_PageContentControl.Content = page;
        }

        public static void SwitchToSubpage(object subpage)
        {
            m_SubPageContentControl.Content = subpage;
        }
    }
}

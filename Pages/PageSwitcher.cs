using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace AnonymDesktopClient.Pages
{
    static class PageSwitcher
    {
        static public MetroContentControl ContentControl
        {
            set { m_ContentControl = value; }
        }

        static private MetroContentControl m_ContentControl;

        public static void SwitchToPage(object page)
        {
            m_ContentControl.Content = page;
        }
    }
}

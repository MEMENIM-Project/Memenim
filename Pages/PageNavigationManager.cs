using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
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

        static public object MainWindowContext;

        static public IDialogCoordinator DialogCoordinator
        {
            set { m_DialogCoordinator = value; }
            get { return m_DialogCoordinator; }
        }

        static private MetroContentControl m_PageContentControl;
        static private MetroContentControl m_SubPageContentControl;

        static private IDialogCoordinator m_DialogCoordinator;


        public static void SwitchToPage(object page)
        {
            m_PageContentControl.Content = page;
        }

        public static void SwitchToSubpage(object subpage)
        {
            m_SubPageContentControl.Content = subpage;
        }

        public static async void ShowDefaultMessage(string title, string content)
        {
            await m_DialogCoordinator.ShowMessageAsync(MainWindowContext, title, content);
        }
    }
}

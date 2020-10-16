using System;
using System.Collections.Generic;
using System.Diagnostics;
using MahApps.Metro.Controls;
using Memenim.Pages;

namespace Memenim.Managers
{
    static class PageNavigationManager
    {

        private static Dictionary<Type,Page> m_Pages = new Dictionary<Type, Page>();

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
        

        public static void SwitchToPage<T>() where T : Page
        {
            Page pg = GetPageObject<T>();
            m_PageContentControl.Content = pg;
        }

        public static void SwitchToSubpage<T>() where T : Page
        {
            Page pg = GetPageObject<T>();
            m_SubPageContentControl.Content = pg;
        }

        private static Page GetPageObject<T>() where T : Page
        {
            Page pg = null;
            m_Pages.TryGetValue(typeof(T), out pg);
            if (pg == null)
            {
                pg = (Activator.CreateInstance<T>() as Page);
                if (pg != null)
                {
                    m_Pages.Add(typeof(T), pg);
                }
                else
                {
                    Debug.Assert(false, "Failed to create a page");
                }
            }
            return pg;
        }

        public static Page GetPage<T>() where T : Page
        {
            return GetPageObject<T>();
        }

        public static void GoBack()
        {
            //UserControl backPage = GeneralBlackboard.TryGetValue<UserControl>(BlackBoardValues.EBackPage);
            //if (backPage != null)
            //{
            //    SwitchToSubpage(backPage);
            //}
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Memenim.Pages;

namespace Memenim.Managers
{
    static class PageStorage
    {
        private static Dictionary<Type,PageContent> m_Pages = new Dictionary<Type, PageContent>();
        
        private static PageContent FindPageInternal(Type pageType)
        {
            Debug.Assert(pageType.BaseType.Equals(typeof(PageContent)), "Page class should derive from PageContent");
            PageContent pg = null;
            m_Pages.TryGetValue(pageType, out pg);
            if (pg == null)
            {
                CreatePage(pageType);
            }
            return pg;
        }


        private static PageContent CreatePage(Type pageType)
        {
            PageContent pg = Activator.CreateInstance(pageType) as PageContent;
            if (pg != null)
            {
                m_Pages.Add(pageType, pg);
            }
            else
            {
                Debug.Assert(false, "Failed to create a page");
            }
            return pg;
        }

        public static PageContent GetPage<T>() where T : PageContent
        {
            return FindPageInternal(typeof(T));
        }

        public static PageContent GetPage(Type pageType) 
        {
            return FindPageInternal(pageType);
        }
    }
}

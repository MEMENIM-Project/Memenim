using System;
using System.Collections.Generic;
using System.Windows;
using Memenim.Pages;

namespace Memenim.Navigation
{
    public static class PageStorage
    {
        private static Dictionary<Type, PageContent> _pagesInstances = new Dictionary<Type, PageContent>();

        private static PageContent FindPageInternal(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
                return null;

            _pagesInstances.TryGetValue(type, out PageContent page);

            return page ?? CreatePage(type);
        }

        private static PageContent CreatePage(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
                return null;

            PageContent page = Activator.CreateInstance(type) as PageContent;

            if (page != null)
                _pagesInstances.Add(type, page);

            return page;
        }

        public static PageContent GetPage<T>()
            where T : PageContent
        {
            return FindPageInternal(typeof(T));
        }
        public static PageContent GetPage(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
                return null;

            return FindPageInternal(type);
        }
    }
}

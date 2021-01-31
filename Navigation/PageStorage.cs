using System;
using System.Collections.Generic;
using Memenim.Pages;
using RIS;

namespace Memenim.Navigation
{
    public static class PageStorage
    {
        private static Dictionary<Type, PageContent> _pagesInstances = new Dictionary<Type, PageContent>();

        static PageStorage()
        {
            CreatePage(typeof(LoginPage));
        }

        private static PageContent FindPage(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException("The page class must be derived from the PageContent", nameof(type));
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            _pagesInstances.TryGetValue(type, out PageContent page);

            return page ?? CreatePage(type);
        }

        private static PageContent CreatePage(Type type)
        {
            if (!typeof(PageContent).IsAssignableFrom(type))
            {
                var exception =
                    new ArgumentException("The page class must be derived from the PageContent", nameof(type));
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            PageContent page;

            try
            {
                page = Activator.CreateInstance(type) as PageContent;

                if (page == null)
                    throw new TypeLoadException();
            }
            catch (Exception)
            {
                var exception = new TypeLoadException("Failed to create a page");
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            _pagesInstances.Add(type, page);

            return page;
        }

        public static PageContent GetPage<T>()
            where T : PageContent
        {
            return FindPage(typeof(T));
        }
        public static PageContent GetPage(Type type)
        {
            return FindPage(type);
        }
    }
}

using System;
using Memenim.Pages;

namespace Memenim.Navigation
{
    public sealed class NavigationHistoryNode
    {
        public PageContent Content { get; set; }
        public PageContent ContentContext { get; set; }
        public PageContentType Type { get; set; }
    }
}

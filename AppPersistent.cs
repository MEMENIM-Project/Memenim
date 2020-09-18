using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymDesktopClient.Core
{
    public static class AppPersistent
    {
        public static string UserToken { get; set; }
        public static int LocalUserId { get; set; }
    }
}

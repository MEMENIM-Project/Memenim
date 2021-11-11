using System;

namespace Memenim.Utils
{
    public static class MemenimProtocolApiUtils
    {
        public static string GetUserLink(int id)
        {
            return $"memenim://app/v2/user/show?id={id}";
        }

        public static string GetPostLink(int id)
        {
            return $"memenim://app/v2/post/show?id={id}";
        }

        public static string GetCreateHashFilesLink()
        {
            return "memenim://app/v2/hash/files/create";
        }
    }
}

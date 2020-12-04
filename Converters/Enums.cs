using System;

namespace Memenim.Converters
{
    public enum PostStatusType : byte
    {
        Premoderating = 0,
        Published = 1,
        Rejected = 2
    }

    public enum UserStatusType : byte
    {
        Active = 0,
        Banned = 2,
        Moderator = 9,
        Admin = 10
    }

    public enum ProfileStatSexType : byte
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
    }
}

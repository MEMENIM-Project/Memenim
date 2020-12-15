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

    public enum ProfileStatPurposeType : byte
    {
        Unknown = 0,
        FindFriends = 1,
        FindPenPals = 2,
        FindConversation = 3,
        FindLikeMinded = 4,
        FindSoulMate = 5,
        FindYourself = 6,
        FindPurpose = 7,
        HelpingOther = 8,
        FindUnderstanding = 9,
        ShareInteresting = 10,
        BeHonest = 11,
        ChangeWorld = 12,
        Other = 13
    }

    public enum ProfileStatSexType : byte
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
    }
}

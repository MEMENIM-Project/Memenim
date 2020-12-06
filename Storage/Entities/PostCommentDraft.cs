using System;

namespace Memenim.Storage.Entities
{
    public class PostCommentDraft
    {
        public uint UserId { get; set; }
        public uint PostId { get; set; }
        public string CommentText { get; set; }
        public bool IsAnonymous { get; set; }
    }
}

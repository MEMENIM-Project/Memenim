using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymDesktopClient.DataStructs
{
    class UserData
    {
        public int id { get; set; }
        public string token { get; set; }
    }

    public class PhotoData
    {
        public string photo_big { get; set; }
        public string photo_medium { get; set; }
        public string photo_small { get; set; }
    }

    public class AttachmentData
    {
        public PhotoData photo { get; set; }
    }

    public class PostData
    {
        public int id { get; set; }
        public string text { get; set; }
        public string owner_name { get; set; }
        public int? owner_id { get; set; }
        public int hidden { get; set; }
        public long date { get; set; }
        public List<AttachmentData> attachments { get; set; }
    }

    class CommentData
    {
        public class CommentUserData
        {
            public int id { get; set; }
            public string name { get; set; }
            public string photo { get; set; }
        }

        public int id { get; set; }
        public string text { get; set; }
        public CommentUserData user { get; set; }
    }

    class ProfileData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string think { get; set; }
        public int target { get; set; }
        public string dream { get; set; }
        public string hvalues { get; set; }
        public string interests { get; set; }
        public string fmusic { get; set; }
        public string ffilms { get; set; }
        public string fbooks { get; set; }
        public string about { get; set; }
        public string tempo { get; set; }
        public string attitude { get; set; }
        public int age { get; set; }
        public int sex { get; set; }
        public int country { get; set; }
        public string city { get; set; }
        public string photo { get; set; }
        public string banner { get; set; }
    }

}

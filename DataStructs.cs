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

    public class RectData
    {
        public int width { get; set; }
        public int height { get; set; }
    }
    
    public class SizeData
    {
        public RectData photo_small { get; set; }
        public RectData photo_medium { get; set; }
        public RectData photo_big { get; set; }
    }

    public class PhotoData
    {
        public string photo_big { get; set; }
        public string photo_medium { get; set; }
        public string photo_small { get; set; }
        public SizeData size { get; set; }
    }

    public class AttachmentData
    {
        public string link { get; set; } = null;
        public PhotoData photo { get; set; }
        public string type { get; set; } = "photo";
    }

    public class PostData
    {
        public int id { get; set; }
        public string text { get; set; }
        public string owner_name { get; set; }
        public int? owner_id { get; set; }
        public int adult { get; set; }
        public int open_comments { get; set; } = 1;
        public int category { get; set; } = 6;
        public int hidden { get; set; }
        public int filter { get; set; }
        public int type { get; set; } = 1;
        public long date { get; set; }
        public int author_watch { get; set; }
        public int reposts { get; set; }
        public StatData postviews { get; set; }
        public StatData likes { get; set; }
        public StatData dislikes { get; set; }
        public StatData comments { get; set; }
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

    public class StatData
    {
        public int count { get; set; }
    }

    public class PostRequest
    {
        public enum EPostType
        {
            New = 1,
            Popular = 2,
            Favorite = 3,
            My = 4
        }
        public int count { get; set; } = 20;
        public EPostType type { get; set; } = EPostType.Popular;
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


    class EditPostData
    {
        public int id { get; set; }
        public string text { get; set; }
    }
}

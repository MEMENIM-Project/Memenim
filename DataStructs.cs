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

    class PostData
    {
        public int id { get; set; }
        public string text { get; set; }
        public string owner_name { get; set; }
        public int owner_id { get; set; }
        public long date { get; set; }
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

}

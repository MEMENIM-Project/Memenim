using System;

namespace Memenim.Utils
{
    public class UserNameChangedEventArgs : EventArgs
    {
        public string OldName { get; }
        public string NewName { get; }
        public int UserId { get; }



        public UserNameChangedEventArgs(
            string oldName, string newName, int userId)
        {
            OldName = oldName;
            NewName = newName;
            UserId = userId;
        }
    }

    public class UserPhotoChangedEventArgs : EventArgs
    {
        public string OldPhoto { get; }
        public string NewPhoto { get; }
        public int UserId { get; }




        public UserPhotoChangedEventArgs(
            string oldPhoto, string newPhoto, int userId)
        {
            OldPhoto = oldPhoto;
            NewPhoto = newPhoto;
            UserId = userId;
        }
    }
}

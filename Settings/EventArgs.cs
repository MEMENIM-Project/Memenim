using System;
using Memenim.Settings.Entities;

namespace Memenim.Settings
{
    public class UserChangedEventArgs : EventArgs
    {
        public User OldUser { get; }
        public User NewUser { get; }

        public UserChangedEventArgs(User oldUser, User newUser)
        {
            OldUser = oldUser;
            NewUser = newUser;
        }
    }
}

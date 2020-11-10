using System;
using Memenim.Core.Schema;

namespace Memenim.Pages.ViewModel
{
    public class UserProfileViewModel : PageViewModel
    {
        private ProfileSchema _currentProfileData = new ProfileSchema
        {
            id = -1
        };
        public ProfileSchema CurrentProfileData
        {
            get
            {
                return _currentProfileData;
            }
            set
            {
                _currentProfileData = value;
                OnPropertyChanged(nameof(CurrentProfileData));
            }
        }

        public UserProfileViewModel()
            : base(typeof(UserProfilePage))
        {

        }
    }
}

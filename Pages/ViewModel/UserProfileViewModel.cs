using System;
using System.ComponentModel;
using Memenim.Core.Schema;
using Memenim.Settings;

namespace Memenim.Pages.ViewModel
{
    public class UserProfileViewModel : PageViewModel
    {
        private ProfileSchema _currentProfileData;
        public ProfileSchema CurrentProfileData
        {
            get
            {
                return _currentProfileData;
            }
            set
            {
                if (_currentProfileData != null)
                    _currentProfileData.PropertyChanged -= CurrentProfileData_PropertyChanged;

                _currentProfileData = value;

                if(_currentProfileData != null)
                    _currentProfileData.PropertyChanged += CurrentProfileData_PropertyChanged;

                OnPropertyChanged(nameof(CurrentProfileData));
                OnPropertyChanged(nameof(EditAllowed));
            }
        }
        
        public bool EditAllowed
        {
            get
            {
                return CurrentProfileData.Id == SettingsManager
                    .PersistentSettings.CurrentUser.Id;
            }
        }



        public UserProfileViewModel()
            : base(typeof(UserProfilePage))
        {
            _currentProfileData = new ProfileSchema
            {
                Id = -1
            };

            CurrentProfileData.PropertyChanged += CurrentProfileData_PropertyChanged;
        }



        private void CurrentProfileData_PropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName)
                || e.PropertyName == "id")
            {
                OnPropertyChanged(nameof(EditAllowed));
            }
        }
    }
}

using System;
using Memenim.Core.Schema;
using Memenim.Settings;

namespace Memenim.Pages.ViewModel
{
    public class SubmitPostViewModel : PageViewModel
    {
        private PostSchema _currentPostData = new PostSchema
        {
            OwnerId = SettingsManager.PersistentSettings.CurrentUser.Id
        };
        public PostSchema CurrentPostData
        {
            get
            {
                return _currentPostData;
            }
            set
            {
                _currentPostData = value;
                OnPropertyChanged(nameof(CurrentPostData));
            }
        }

        public SubmitPostViewModel()
            : base(typeof(SubmitPostPage))
        {

        }
    }
}

using System;
using Memenim.Core.Schema;
using Memenim.Settings;

namespace Memenim.Pages.ViewModel
{
    public class SubmitPostViewModel : PageViewModel
    {
        private PostSchema _currentPostData = new PostSchema
        {
            owner_id = SettingsManager.PersistentSettings.CurrentUserId
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

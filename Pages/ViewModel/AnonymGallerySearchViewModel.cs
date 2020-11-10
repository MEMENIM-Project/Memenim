using System;
using System.Threading.Tasks;

namespace Memenim.Pages.ViewModel
{
    public class AnonymGallerySearchViewModel : PageViewModel
    {
        private Func<string, Task> _onPicSelect = _ => Task.CompletedTask;
        public Func<string, Task> OnPicSelect
        {
            get
            {
                return _onPicSelect;
            }
            set
            {
                _onPicSelect = value;
                OnPropertyChanged(nameof(OnPicSelect));
            }
        }

        public AnonymGallerySearchViewModel()
            : base(typeof(AnonymGallerySearchPage))
        {

        }
    }
}

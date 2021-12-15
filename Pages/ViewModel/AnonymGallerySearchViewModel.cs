using System;
using System.Threading.Tasks;

namespace Memenim.Pages.ViewModel
{
    public class AnonymGallerySearchViewModel : PageViewModel
    {
        private Func<string, Task> _imageSelectionDelegate;
        public Func<string, Task> ImageSelectionDelegate
        {
            get
            {
                return _imageSelectionDelegate;
            }
            set
            {
                _imageSelectionDelegate = value;
                OnPropertyChanged(nameof(ImageSelectionDelegate));
            }
        }



        public AnonymGallerySearchViewModel()
            : base(typeof(AnonymGallerySearchPage))
        {
            _imageSelectionDelegate = _ => Task.CompletedTask;
        }
    }
}

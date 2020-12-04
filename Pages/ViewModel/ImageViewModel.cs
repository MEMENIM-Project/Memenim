using System;
using System.Collections.Generic;
using System.Text;

namespace Memenim.Pages.ViewModel
{
    public class ImagePreviewViewModel : PageViewModel
    {
        private string _ImageSource;
        public string ImageSource
        {
            get 
            {
                return _ImageSource;
            }
            set 
            {
                _ImageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        public ImagePreviewViewModel()
            : base(typeof(ImagePreviewPage))
        {
            
        }
    }
}

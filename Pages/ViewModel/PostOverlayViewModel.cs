using System;
using Memenim.Core.Schema;

namespace Memenim.Pages.ViewModel
{
    public class PostOverlayViewModel : PageViewModel
    {
        private PostSchema _currentPostData = new PostSchema();
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
        private double _scrollOffset;
        public double ScrollOffset
        {
            get
            {
                return _scrollOffset;
            }
            set
            {
                _scrollOffset = value;
                OnPropertyChanged(nameof(ScrollOffset));
            }
        }

        public PostOverlayViewModel()
            : base(typeof(PostOverlayPage))
        {

        }
    }
}

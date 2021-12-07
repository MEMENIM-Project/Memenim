using System;
using Memenim.Core.Schema;
using Memenim.Widgets;

namespace Memenim.Pages.ViewModel
{
    public class PostOverlayViewModel : PageViewModel
    {
        private Post _sourcePost;
        public Post SourcePost
        {
            get
            {
                return _sourcePost;
            }
            set
            {
                _sourcePost = value;
                OnPropertyChanged(nameof(SourcePost));
            }
        }
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

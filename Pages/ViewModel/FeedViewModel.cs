using System;
using System.Windows.Input;
using Memenim.Commands;

namespace Memenim.Pages.ViewModel
{
    public class FeedViewModel : PageViewModel
    {
        private ICommand _onPostScrollEnd;
        public ICommand OnPostScrollEnd
        {
            get
            {
                return _onPostScrollEnd;
            }
            set
            {
                _onPostScrollEnd = value;
                OnPropertyChanged(nameof(OnPostScrollEnd));
            }
        }
        private int _offset;
        public int Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }
        private int _lastNewHeadPostId;
        public int LastNewHeadPostId
        {
            get
            {
                return _lastNewHeadPostId;
            }
            set
            {
                _lastNewHeadPostId = value;
                OnPropertyChanged(nameof(LastNewHeadPostId));
            }
        }
        private int _newPostsCount;
        public int NewPostsCount
        {
            get
            {
                return _newPostsCount;
            }
            set
            {
                _newPostsCount = value;
                OnPropertyChanged(nameof(NewPostsCount));
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



        public FeedViewModel()
            : base(typeof(FeedPage))
        {
            _onPostScrollEnd = new AsyncBasicCommand();
            _lastNewHeadPostId = -1;
        }
    }
}

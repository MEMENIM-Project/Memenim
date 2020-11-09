using System;
using System.Windows.Input;
using Memenim.Commands;

namespace Memenim.Pages.ViewModel
{
    public class FeedViewModel : PageViewModel
    {
        private ICommand _onPostScrollEnd = new BasicCommand(_ => false);
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

        public FeedViewModel()
            : base(typeof(FeedPage))
        {

        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Memenim.Commands;

namespace Memenim.Pages.ViewModel
{
    public class TenorSearchViewModel : PageViewModel
    {
        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand;
            }
            set
            {
                _searchCommand = value;
                OnPropertyChanged(nameof(SearchCommand));
            }
        }
        private string _searchText;
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }
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



        public TenorSearchViewModel()
            : base(typeof(TenorSearchPage))
        {
            _searchCommand = new AsyncBasicCommand();
            _searchText = string.Empty;
            _imageSelectionDelegate = _ => Task.CompletedTask;
        }
    }
}

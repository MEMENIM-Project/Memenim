using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Memenim.Commands;

namespace Memenim.Pages.ViewModel
{
    public class TenorSearchViewModel : PageViewModel
    {
        private ICommand _searchCommand = new BasicCommand(_ => false);
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
        private string _searchText = string.Empty;
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

        public TenorSearchViewModel()
            : base(typeof(TenorSearchPage))
        {

        }
    }
}

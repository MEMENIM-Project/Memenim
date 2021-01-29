using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RIS;

namespace Memenim.Pages.ViewModel
{
    public abstract class PageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Type PageType { get; }

        protected PageViewModel(Type pageType)
        {
            if (!typeof(PageContent).IsAssignableFrom(pageType))
            {
                var exception =
                    new ArgumentException("The page class must be derived from the PageContent", nameof(pageType));
                Events.OnError(null, new RErrorEventArgs(exception, exception.Message, exception.StackTrace));
                throw exception;
            }

            PageType = pageType;
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

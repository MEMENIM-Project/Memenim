using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MahApps.Metro.IconPacks;

namespace Memenim.TabLayouts.NavigationBar
{
    public sealed class NavRedirectButtonNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _pageName = string.Empty;
        public string PageName
        {
            get
            {
                return _pageName;
            }
            set
            {
                _pageName = value;
                OnPropertyChanged(nameof(PageName));
            }
        }
        private PackIconModernKind _iconKind = PackIconModernKind.Xbox;
        public PackIconModernKind IconKind
        {
            get
            {
                return _iconKind;
            }
            set
            {
                _iconKind = value;
                OnPropertyChanged(nameof(IconKind));
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

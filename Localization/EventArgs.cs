using System;
using Memenim.Localization.Entities;

namespace Memenim.Localization
{
    public class LanguageChangedEventArgs : EventArgs
    {
        public LocalizationXamlModule OldLanguage { get; }
        public LocalizationXamlModule NewLanguage { get; }

        public LanguageChangedEventArgs(LocalizationXamlModule oldLanguage,
            LocalizationXamlModule newLanguage)
        {
            OldLanguage = oldLanguage;
            NewLanguage = newLanguage;
        }
    }
}

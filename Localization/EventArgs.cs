using System;
using Memenim.Localization.Entities;

namespace Memenim.Localization
{
    public class LanguageChangedEventArgs : EventArgs
    {
        public LocalizationXamlFile OldLanguage { get; }
        public LocalizationXamlFile NewLanguage { get; }

        public LanguageChangedEventArgs(LocalizationXamlFile oldLanguage,
            LocalizationXamlFile newLanguage)
        {
            OldLanguage = oldLanguage;
            NewLanguage = newLanguage;
        }
    }
}

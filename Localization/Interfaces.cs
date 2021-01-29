using System;
using System.Collections.ObjectModel;
using Memenim.Localization.Entities;

namespace Memenim.Localization
{
    public interface ILocalizable
    {
        ReadOnlyDictionary<string, LocalizationXamlFile> Locales { get; }
    }
}

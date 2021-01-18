using System;
using System.Collections.ObjectModel;

namespace Memenim.Localization
{
    public interface ILocalizable
    {
        ReadOnlyDictionary<string, string> Locales { get; }
    }
}

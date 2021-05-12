using System;
using Memenim.Scripting.Core.Entities;
using Memenim.Utils;

namespace Memenim.Script.Implementations
{
    public class MemenimLocalization : MemenimLocalizationBase
    {
        public override string GetLocalized(string key)
        {
            return GetLocalized<string>(key);
        }
        public override TOut GetLocalized<TOut>(string key)
        {
            return LocalizationUtils.GetLocalized<TOut>(key);
        }

        public override string TryGetLocalized(string key)
        {
            return TryGetLocalized<string>(key);
        }
        public override TOut TryGetLocalized<TOut>(string key)
        {
            return LocalizationUtils.TryGetLocalized<TOut>(key);
        }
    }
}

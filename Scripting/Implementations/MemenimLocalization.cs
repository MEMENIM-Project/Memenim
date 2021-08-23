using System;
using Memenim.Scripting.Core.Entities;
using Memenim.Utils;

namespace Memenim.Scripting.Implementations
{
    public class MemenimLocalization : MemenimLocalizationBase
    {
        public override string GetLocalized(
            string key)
        {
            return LocalizationUtils.GetLocalized(
                key);
        }

        public override bool TryGetLocalized(
            string key, out string value)
        {
            return LocalizationUtils.TryGetLocalized(
                key, out value);
        }
    }
}

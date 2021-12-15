using System;
using Memenim.Utils;

namespace Memenim.Extensions
{
    public static class EnumExtensions
    {
        public static string GetResourceKey(this Enum source)
        {
            return $"Enum-{source.GetType().Name}-{source}";
        }
        public static string GetResourceKey(this Enum source,
            string name)
        {
            return $"Enum-{source.GetType().Name}-{name}";
        }

        public static string GetLocalizedName(this Enum source)
        {
            var name = Enum.GetName(
                source.GetType(), source);
            var localizedName = LocalizationUtils
                .GetLocalized(GetResourceKey(source));

            return !string.IsNullOrEmpty(localizedName)
                ? localizedName
                : name;
        }

        public static string[] GetLocalizedNames(this Enum source)
        {
            var names = Enum.GetNames(
                source.GetType());
            var localizedNames =
                new string[names.Length];

            for (var i = 0; i < names.Length; ++i)
            {
                ref var name = ref names[i];

                var localizedName = LocalizationUtils
                    .GetLocalized(GetResourceKey(source, name));

                localizedNames[i] = !string.IsNullOrEmpty(localizedName)
                    ? localizedName
                    : name;
            }

            return localizedNames;
        }

        public static T ParseLocalizedName<T>(this T source,
            string localizedName)
            where T: struct, Enum
        {
            var names = Enum.GetNames(
                source.GetType());
            var localizedNames =
                source.GetLocalizedNames();

            for (var i = 0; i < localizedNames.Length; ++i)
            {
                if (localizedNames[i] != localizedName)
                    continue;

                ref var name = ref names[i];

                var value = Enum.Parse<T>(
                    name);

                return value;
            }

            return default;
        }
    }
}

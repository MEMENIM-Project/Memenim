using System;

namespace Memenim.Extensions
{
    public static class EnumExtensions
    {
        public static string GetResourceKey(this Enum targetEnum)
        {
            return $"Enum-{targetEnum.GetType().Name}-{targetEnum}";
        }
        public static string GetResourceKey(this Enum targetEnum, string name)
        {
            return $"Enum-{targetEnum.GetType().Name}-{name}";
        }

        public static string GetLocalizedName(this Enum targetEnum)
        {
            string name =
                Enum.GetName(targetEnum.GetType(), targetEnum);
            string localizedName =
                MainWindow.Instance.FindResource(GetResourceKey(targetEnum)).ToString();

            return !string.IsNullOrEmpty(localizedName)
                ? localizedName
                : name;
        }

        public static string[] GetLocalizedNames(this Enum targetEnum)
        {
            var names = Enum.GetNames(targetEnum.GetType());
            var localizedNames = new string[names.Length];

            for (var i = 0; i < names.Length; ++i)
            {
                ref var name = ref names[i];

                string localizedName =
                    MainWindow.Instance.FindResource(GetResourceKey(targetEnum, name)).ToString();

                localizedNames[i] = !string.IsNullOrEmpty(localizedName)
                    ? localizedName
                    : name;
            }

            return localizedNames;
        }

        public static T ParseLocalizedName<T>(this Enum targetEnum, string localizedName)
            where T: struct
        {
            var names = Enum.GetNames(targetEnum.GetType());
            var localizedNames = targetEnum.GetLocalizedNames();

            for (int i = 0; i < localizedNames.Length; ++i)
            {
                if (localizedNames[i] != localizedName)
                    continue;

                ref var name = ref names[i];

                T value = Enum.Parse<T>(name);

                return value;
            }

            return default(T);
        }
    }
}

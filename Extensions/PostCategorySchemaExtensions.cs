using System;
using System.Linq;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Utils;

namespace Memenim.Extensions
{
    public static class PostCategorySchemaExtensions
    {
        public static readonly PostCategorySchema[] Categories;



        static PostCategorySchemaExtensions()
        {
            Categories = PostApi.PostCategories
                .Values.ToArray();
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Global

        public static string GetResourceKey(this PostCategorySchema source)
        {
            return $"Schema-{source.GetType().Name}-{source.Id}";
        }
        public static string GetResourceKey(this PostCategorySchema source, int id)
        {
            return $"Schema-{source.GetType().Name}-{id}";
        }

        public static string GetLocalizedName(this PostCategorySchema source)
        {
            var name =
                source.Name;
            var localizedName = LocalizationUtils
                .GetLocalized(GetResourceKey(source));

            return !string.IsNullOrEmpty(localizedName)
                ? localizedName
                : name;
        }

        public static string[] GetLocalizedNames(this PostCategorySchema source)
        {
            return GetLocalizedNames();
        }
        public static string[] GetLocalizedNames()
        {
            var localizedNames =
                new string[Categories.Length];

            for (var i = 0; i < Categories.Length; ++i)
            {
                ref var category = ref Categories[i];

                var name =
                    category.Name;
                var localizedName = LocalizationUtils
                    .GetLocalized(GetResourceKey(category));

                localizedNames[i] = !string.IsNullOrEmpty(localizedName)
                    ? localizedName
                    : name;
            }

            return localizedNames;
        }

        public static PostCategorySchema ParseLocalizedName(this PostCategorySchema source,
            string localizedName)
        {
            return ParseLocalizedName(localizedName);
        }
        public static PostCategorySchema ParseLocalizedName(string localizedName)
        {
            var localizedNames = GetLocalizedNames();

            for (var i = 0; i < localizedNames.Length; ++i)
            {
                if (localizedNames[i] != localizedName)
                    continue;

                return Categories[i];
            }

            return new PostCategorySchema
            {
                Id = -1,
                Name = null
            };
        }

        // ReSharper restore UnusedParameter.Global
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр
    }
}

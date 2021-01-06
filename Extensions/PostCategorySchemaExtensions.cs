using System;
using System.Linq;
using Memenim.Core.Api;
using Memenim.Core.Schema;

namespace Memenim.Extensions
{
    public static class PostCategorySchemaExtensions
    {
        public static string GetResourceKey(this PostCategorySchema targetSchema)
        {
            return $"Schema-{targetSchema.GetType().Name}-{targetSchema.id}";
        }
        public static string GetResourceKey(this PostCategorySchema targetSchema, int id)
        {
            return $"Schema-{targetSchema.GetType().Name}-{id}";
        }

        public static string GetLocalizedName(this PostCategorySchema targetSchema)
        {
            string name =
                targetSchema.text;
            string localizedName =
                MainWindow.Instance.FindResource(GetResourceKey(targetSchema)).ToString();

            return !string.IsNullOrEmpty(localizedName)
                ? localizedName
                : name;
        }

        public static string[] GetLocalizedNames(this PostCategorySchema targetSchema)
        {
            return GetLocalizedNames();
        }
        public static string[] GetLocalizedNames()
        {
            var categories = PostApi.PostCategories.Values.ToArray();
            var localizedNames = new string[categories.Length];

            for (var i = 0; i < categories.Length; ++i)
            {
                ref var category = ref categories[i];

                string name = category.text;

                string localizedName =
                    MainWindow.Instance.FindResource(GetResourceKey(category)).ToString();

                localizedNames[i] = !string.IsNullOrEmpty(localizedName)
                    ? localizedName
                    : name;
            }

            return localizedNames;
        }

        public static PostCategorySchema ParseLocalizedName(this PostCategorySchema targetSchema,
            string localizedName)
        {
            return ParseLocalizedName(localizedName);
        }
        public static PostCategorySchema ParseLocalizedName(string localizedName)
        {
            var categories = PostApi.PostCategories.Values.ToArray();
            var localizedNames = GetLocalizedNames();

            for (int i = 0; i < localizedNames.Length; ++i)
            {
                if (localizedNames[i] != localizedName)
                    continue;

                return categories[i];
            }

            return new PostCategorySchema
            {
                id = 0,
                text = null
            };
        }
    }
}

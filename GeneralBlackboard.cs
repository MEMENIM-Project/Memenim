using System.Collections.Generic;

namespace Memenim
{
    public enum BlackBoardValues
    {
        EPageToRedirect,
        EBackPage
    }

    public static class GeneralBlackboard
    {
        private static readonly Dictionary<BlackBoardValues, object> BoardValues;

        static GeneralBlackboard()
        {
            BoardValues = new Dictionary<BlackBoardValues, object>();
        }

        public static object TryGetValue(BlackBoardValues key)
        {
            BoardValues.TryGetValue(key, out var value);
            return value;
        }
        public static T TryGetValue<T>(BlackBoardValues key)
        {
            BoardValues.TryGetValue(key, out var value);
            return (T)value;
        }

        public static void SetValue(BlackBoardValues key, object value)
        {
            BoardValues[key] = value;
        }
    }
}

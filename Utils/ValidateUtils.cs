using System;
using System.Text.RegularExpressions;

namespace Memenim.Utils
{
    public static class ValidateUtils
    {
        public static bool IsEmptyString(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsFixedLengthString(string value, int length)
        {
            return value.Length == length;
        }

        public static bool IsValidCharsString(string value, string regexCharPattern,
            RegexOptions regexOptions = RegexOptions.None)
        {
            return Regex.Matches(value, $"[^{regexCharPattern}]", regexOptions).Count == 0;
        }

        public static bool IsValidPatternString(string value, string regexStringPattern,
            RegexOptions regexOptions = RegexOptions.None)
        {
            return Regex.Matches(value, $"{regexStringPattern}", regexOptions).Count == 1;
        }
    }
}

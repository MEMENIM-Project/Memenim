﻿using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Utils;

namespace Memenim.Converters
{
    public sealed class BirthDateToAgeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ulong result = 0;

                if (value is ulong longValue)
                    result = longValue;

                if (result == 0)
                    return string.Empty;

                var birthDateTime = TimeUtils.ToDateTime(result);
#pragma warning disable SS002 // DateTime.Now was referenced
                var age = DateTime.Now.Year - birthDateTime.Year;
#pragma warning restore SS002 // DateTime.Now was referenced

                return age.ToString();

            }
            catch (Exception)
            {
                return "0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}

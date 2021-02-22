using System;

namespace Memenim.Utils
{
    public static class TimeUtils
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public static long ToUnixTimeStamp(DateTime time)
        {
            return (long)time
                .ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;
        }
    }
}

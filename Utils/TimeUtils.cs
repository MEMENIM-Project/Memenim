using System;

namespace Memenim.Utils
{
    public static class TimeUtils
    {
        public static DateTime ToDateTime(ulong timeStamp)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return time
                .AddSeconds(timeStamp)
                .ToLocalTime();
        }

        public static ulong ToUnixTimeStamp(DateTime time)
        {
            return (ulong)time
                .ToUniversalTime()
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;
        }
    }
}

using System;
using static Toggl.Multivac.Math;

namespace Toggl.Multivac.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static long ToUnixTimeSeconds(this DateTimeOffset dateTime)
        {
            // constant and implementation taken from .NET reference source:
            // https://referencesource.microsoft.com/#mscorlib/system/datetimeoffset.cs
            const long unixEpochSeconds = 62_135_596_800L;

            var seconds = dateTime.UtcDateTime.Ticks / TimeSpan.TicksPerSecond;
            return seconds - unixEpochSeconds;
        }

        public static DateTimeOffset RoundToClosestMinute(this DateTimeOffset time)
            => time.Second >= (SecondsInAMinute / 2)
                ? time + TimeSpan.FromSeconds(SecondsInAMinute - time.Second)
                : time - TimeSpan.FromSeconds(time.Second);

        public static DateTimeOffset WithDate(this DateTimeOffset original, DateTimeOffset date) 
            => new DateTimeOffset(date.Year, date.Month, date.Day,
                                  original.Hour, original.Minute, original.Second, original.Offset);

        public static DateTimeOffset WithTime(this DateTimeOffset original, DateTimeOffset time)
            => new DateTimeOffset(original.Year, original.Month, original.Day,
                                  time.Hour, time.Minute, time.Second, original.Offset);
    }
}

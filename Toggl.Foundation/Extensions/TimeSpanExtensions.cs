using System;
using Toggl.Multivac;

namespace Toggl.Foundation.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToFormattedString(this TimeSpan duration, DurationFormat format)
        {
            switch (format)
            {
                case DurationFormat.Classic:
                    return convertToClassicFormat(duration);
                case DurationFormat.Improved:
                    return convertToImprovedFormat(duration);
                case DurationFormat.Decimal:
                    return convertToDecimalFormat(duration);
                default:
                    throw new ArgumentOutOfRangeException(
                        $"The duration converter parameter '{format}' is not of the supported formats.");
            }
        }

        private static string convertToDecimalFormat(TimeSpan value)
            => $"{value.TotalHours:00.00} {Resources.UnitHour}";

        private static string convertToImprovedFormat(TimeSpan value)
            => $@"{(int)value.TotalHours}:{value:mm\:ss}";

        private static string convertToClassicFormat(TimeSpan value)
        {
            if (value >= TimeSpan.FromHours(1))
                return $@"{(int)value.TotalHours:00}:{value:mm\:ss}";

            if (value >= TimeSpan.FromMinutes(1))
                return $@"{value:mm\:ss} {Resources.UnitMin}";

            return $"{value:ss} {Resources.UnitSecond}";
        }
    }
}

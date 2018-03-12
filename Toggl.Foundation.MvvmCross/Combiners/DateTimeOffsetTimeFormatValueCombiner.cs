using System;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Combiners
{
    [Preserve(AllMembers = true)]
    public sealed class DateTimeOffsetTimeFormatValueCombiner : BaseTwoValuesCombiner<DateTimeOffset, TimeFormat>
    {
        private readonly TimeZoneInfo timeZone;

        public DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo timeZone)
        {
            this.timeZone = timeZone;
        }

        protected override object Combine(DateTimeOffset date, TimeFormat format)
            => getDateTimeOffsetInCorrectTimeZone(date).ToString(format.Format);

        private DateTimeOffset getDateTimeOffsetInCorrectTimeZone(DateTimeOffset value)
            => value == default(DateTimeOffset) ? value : TimeZoneInfo.ConvertTime(value, timeZone);
    }
}

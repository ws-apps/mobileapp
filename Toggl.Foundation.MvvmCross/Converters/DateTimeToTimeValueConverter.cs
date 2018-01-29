using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class DateTimeToTimeValueConverter : MvxValueConverter<DateTimeOffset, string>
    {
        protected override string Convert(DateTimeOffset value, Type targetType, object parameter, CultureInfo culture)
            => (value == default(DateTimeOffset) ? value : TimeZoneInfo.ConvertTime(value, TimeZoneInfo.Local)).ToString("HH:mm");
    }
}

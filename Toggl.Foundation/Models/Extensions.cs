using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Models
{
    internal static class Extensions
    {
        public static IDatabasePreferences With(
            this IDatabasePreferences original,
            New<long> id = default(New<long>),
            New<TimeFormat> timeOfDayFormat = default(New<TimeFormat>),
            New<DateFormat> dateFormat = default(New<DateFormat>),
            New<DurationFormat> durationFormat = default(New<DurationFormat>),
            New<bool> collapseTimeEntries = default(New<bool>),
            New<SyncStatus> syncStatus = default(New<SyncStatus>),
            New<string> lastSyncErrorMessage = default(New<string>),
            New<bool> isDeleted = default(New<bool>)
            )
            => new Preferences(
                id.ValueOr(original.Id),
                timeOfDayFormat.ValueOr(original.TimeOfDayFormat),
                dateFormat.ValueOr(original.DateFormat),
                durationFormat.ValueOr(original.DurationFormat),
                collapseTimeEntries.ValueOr(original.CollapseTimeEntries),
                syncStatus.ValueOr(original.SyncStatus),
                lastSyncErrorMessage.ValueOr(original.LastSyncErrorMessage),
                isDeleted.ValueOr(original.IsDeleted)
                );
    }
}

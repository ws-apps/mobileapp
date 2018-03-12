using Toggl.Multivac;

namespace Toggl.Foundation.DTOs
{
    public struct EditPreferencesDTO
    {
        public DateFormat? DateFormat { get; set; }
        public DurationFormat? DurationFormat { get; set; }
        public TimeFormat? TimeOfDayFormat { get; set; }
        public bool? CollapseTimeEntries { get; set; }
    }
}

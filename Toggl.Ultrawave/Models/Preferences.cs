using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Preferences : IPreferences
    {
        [JsonProperty("timeofday_format")]
        public string TimeOfDayFormat { get; set; }

        public string DateFormat { get; set; }

        [JsonConverter(typeof(StringEnumConverter), true)]
        public DurationFormat DurationFormat { get; set; }

        [JsonProperty("CollapseTimeEntries")]
        public bool CollapseTimeEntries { get; set; }
    }
}

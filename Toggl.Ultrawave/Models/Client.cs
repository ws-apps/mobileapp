using System;
using Newtonsoft.Json;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.Models
{
    internal sealed partial class Client : IClient
    {
        public long Id { get; set; }

        [JsonProperty("wid")]
        public long WorkspaceId { get; set; }

        public string Name { get; set; }

        public DateTimeOffset At { get; set; }

        public DateTimeOffset? ServerDeletedAt { get; set; }
    }
}

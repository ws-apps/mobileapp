using System;

namespace Toggl.Foundation.DTOs
{
    public sealed class StartTimeEntryDTO
    {
        public long UserId { get; set; }

        public bool Billable { get; set; }

        public long? ProjectId { get; set; }

        public long? TaskId { get; set; }

        public string Description { get; set; }

        public long WorkspaceId { get; set; }

        public long[] TagIds { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}

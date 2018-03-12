using System;
using Toggl.Multivac;

namespace Toggl.Foundation.Reports
{
    public struct ChartSegment
    {
        public TimeSpan TrackedTime { get; }

        public float BillableSeconds { get; }

        public float Percentage { get; }

        public string ProjectName { get; }

        public string Color { get; }

        public DurationFormat DurationFormat { get; set; }

        public ChartSegment(
            string projectName,
            float percentage,
            float trackedSeconds,
            float billableSeconds,
            string color,
            DurationFormat durationFormat = DurationFormat.Improved)
        {
            ProjectName = projectName;
            Color = color;
            Percentage = percentage;
            TrackedTime = TimeSpan.FromSeconds(trackedSeconds);
            BillableSeconds = billableSeconds;
            DurationFormat = durationFormat;
        }
    }

    public static class ChartSegmentExtensions
    {
        public static ChartSegment WithDurationFormat(this ChartSegment segment, DurationFormat durationFormat)
            => new ChartSegment(
                segment.ProjectName,
                segment.Percentage,
                (float)segment.TrackedTime.TotalSeconds,
                segment.BillableSeconds,
                segment.Color,
                durationFormat);
    }
}

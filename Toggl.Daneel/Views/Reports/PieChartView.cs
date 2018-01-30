using System;
using UIKit;
using System.Linq;
using System.Collections.ObjectModel;
using CoreGraphics;
using Foundation;
using Toggl.Foundation.Reports;
using Toggl.Multivac.Extensions;
using MvvmCross.Platform.UI;
using MvvmCross.Plugins.Color.iOS;

namespace Toggl.Daneel.Views.Reports
{
    [Register(nameof(PieChartView))]
    public sealed class PieChartView : UIView
    {
        private const float padding = 8.0f;
        private const int maxSegmentName = 18;
        private const float textDrawingThreshold = 0.1f;
        private static readonly nfloat pi = (nfloat)(Math.PI);
        private static readonly UIStringAttributes attributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(10, UIFontWeight.Semibold),
            ForegroundColor = UIColor.White
        };

        private ObservableCollection<ChartSegment> segments = new ObservableCollection<ChartSegment>();
        public ObservableCollection<ChartSegment> Segments
        {
            get => segments;
            set
            {
                segments = value;
                SetNeedsDisplay();
            }
        }

        public PieChartView(IntPtr handle) : base(handle)
        {
        }

        public override void Draw(CGRect rect)
        {
            var ctx = UIGraphics.GetCurrentContext();
            if (ctx == null) return;

            var viewCenterX = Bounds.Size.Width * 0.5f;
            var viewCenterY = Bounds.Size.Height * 0.5f;
            var radius = viewCenterX;
            var totalValue = Segments.Select(x => x.TrackedTime.TotalSeconds).Sum();

            var startAngle = pi * -0.5f;

            foreach (var segment in Segments)
            {
                ctx.SetFillColor(MvxColor.ParseHexString(segment.Color).ToNativeColor().CGColor);

                var percent = (nfloat)(segment.TrackedTime.TotalSeconds / totalValue);
                // Calculate end angle
                var endAngle = startAngle + 2 * pi * percent;

                // Draw arc
                ctx.MoveTo(viewCenterX, viewCenterY);
                ctx.AddArc(viewCenterX, viewCenterY, radius, startAngle, endAngle, clockwise: false);
                ctx.FillPath();

                // Disable drawing on segments that are too small
                if (percent > textDrawingThreshold)
                {
                    // Save state for restoring later.
                    ctx.SaveState();

                    // Translate to draw the text
                    ctx.TranslateCTM(viewCenterX, viewCenterY);
                    ctx.RotateCTM(endAngle + pi);

                    // Draw the text
                    var integerPercentage = (int)(percent * 100);
                    var nameToDraw = new NSAttributedString(segment.ProjectName.TruncatedAt(maxSegmentName), attributes);
                    var percentageToDraw = new NSAttributedString($"{integerPercentage}%", attributes);
                    nameToDraw.DrawString(new CGPoint(x: -radius + padding, y: padding));
                    percentageToDraw.DrawString(new CGPoint(x: -radius + padding, y: nameToDraw.Size.Height + padding));

                    // Restore the original coordinate system.
                    ctx.RestoreState();
                }

                startAngle = endAngle;
            }
        }
    }
}
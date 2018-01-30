using CoreGraphics;
using Toggl.Multivac;

namespace Toggl.Daneel.Extensions
{
    public static class PointExtensions
    {
        public static CGPoint ToCGPoint(this Point point)
            => new CGPoint(point.X, point.Y);

        public static Point ToMultivacPoint(this CGPoint point)
            => new Point { X = point.X, Y = point.Y };
    }
}

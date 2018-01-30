using System;
using CoreAnimation;
using CoreGraphics;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using Foundation;
using UIKit;
using MvvmCross.Platform.Core;
using static Toggl.Multivac.Math;
using Toggl.Daneel.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Daneel.Views.EditDuration
{
    [Register(nameof(WheelForegroundView))]
    public sealed class WheelForegroundView : BaseWheelView
    {
        private readonly CGColor backgroundColor = Color.EditDuration.Wheel.Duration.ToNativeColor().CGColor;

        private readonly CGColor capColor = Color.EditDuration.Wheel.Cap.ToNativeColor().CGColor;

        private readonly CGColor shadowColor = Color.EditDuration.Wheel.Shadow.ToNativeColor().CGColor;

        // The sizes are relative to the radius of the wheel.
        // The radius of the wheel in the design document is 128 points.
        private readonly nfloat capRadius = 14.05f / 128f;

        private readonly nfloat horizontalBarLength = 12f / 128f;

        private readonly nfloat horizontalBarHeight = 1.5f / 128f;

        private readonly nfloat horizontalBarsDistance = 3.1f / 128f;

        private readonly nfloat horizontalBarCornerRadius = 0.2f / 128f;

        private readonly nfloat shadowRadius = 4f / 128f;

        private readonly float shadowOpacity = 0.3f;

        private readonly int feedbackEveryMinutes = 1;

        private double endPointsRadius => SmallRadius + (Radius - SmallRadius) / 2;

        private DateTimeOffset startTime;

        private DateTimeOffset endTime;

        private bool isRunning;

        private bool isEnabled;

        private CGPoint startTimePosition;

        private CGPoint endTimePosition;

        private CGPoint roundedStartTimePosition;

        private CGPoint roundedEndTimePosition;

        private UITouch currentTouch;

        private UISelectionFeedbackGenerator feedbackGenerator;

        private WheelUpdateType updateType;

        private int numberOfFullLoops => (int)((EndTime - StartTime).TotalMinutes / MinutesInAnHour);

        private bool isFullCircle => numberOfFullLoops >= 1;

        private double startTimeAngle => startTime.LocalDateTime.TimeOfDay.ToAngleOnTheDial().ToPositiveAngle();

        private double endTimeAngle => endTime.LocalDateTime.TimeOfDay.ToAngleOnTheDial().ToPositiveAngle();

        private double roundedStartTimeAngle => StartTime.LocalDateTime.TimeOfDay.ToAngleOnTheDial().ToPositiveAngle();

        private double roundedEndTimeAngle => EndTime.LocalDateTime.TimeOfDay.ToAngleOnTheDial().ToPositiveAngle();

        public event EventHandler StartTimeChanged;

        public event EventHandler EndTimeChanged;

        public DateTimeOffset MinimumStartTime { get; set; }

        public DateTimeOffset MaximumStartTime { get; set; }

        public DateTimeOffset MinimumEndTime { get; set; }

        public DateTimeOffset MaximumEndTime { get; set; }

        public DateTimeOffset StartTime
        {
            get => roundToLowerMinute(startTime);
            set
            {
                if (startTime == value) return;
                startTime = value.Clamp(MinimumStartTime, MaximumStartTime);
                StartTimeChanged.Raise(this);
                SetNeedsLayout();
            }
        }

        public DateTimeOffset EndTime
        {
            get => roundToLowerMinute(endTime);
            set
            {
                if (endTime == value) return;
                endTime = value.Clamp(MinimumEndTime, MaximumEndTime);
                EndTimeChanged.Raise(this);
                SetNeedsLayout();
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                if (isRunning == value) return;
                isRunning = value;
                SetNeedsLayout();
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                SetNeedsLayout();
            }
        }

        public WheelForegroundView(IntPtr handle) : base(handle)
        {
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            RemoveSublayers();
            calculateEndPointPositions();

            if (isFullCircle)
            {
                var fullWheel = CreateWheelLayer(backgroundColor);
                Layer.AddSublayer(fullWheel);
            }

            var backgroundLayer = createBackgroundLayer();
            Layer.AddSublayer(backgroundLayer);

            if (IsEnabled)
            {
                if (IsRunning == false)
                {
                    var endCap = createCap(roundedEndTimePosition);
                    Layer.AddSublayer(endCap);
                }

                var startCap = createCap(roundedStartTimePosition);
                Layer.AddSublayer(startCap);
            }
        }

        private void calculateEndPointPositions()
        {
            var center = Center.ToMultivacPoint();

            startTimePosition = PointOnCircumference(center, startTimeAngle, endPointsRadius).ToCGPoint();
            endTimePosition = PointOnCircumference(center, endTimeAngle, endPointsRadius).ToCGPoint();

            roundedStartTimePosition = PointOnCircumference(center, roundedStartTimeAngle, endPointsRadius).ToCGPoint();
            roundedEndTimePosition = PointOnCircumference(center, roundedEndTimeAngle, endPointsRadius).ToCGPoint();
        }

        #region Touch interaction

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if (IsEnabled == false) return;

            var touch = findValidTouch(touches);
            if (touch != null)
            {
                currentTouch = touch;
                feedbackGenerator = new UISelectionFeedbackGenerator();
                feedbackGenerator.Prepare();
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if (currentTouch == null) return;

            double previousAngle;
            switch (updateType)
            {
                case WheelUpdateType.EditStartTime:
                    previousAngle = startTimeAngle;
                    break;
                case WheelUpdateType.EditEndTime:
                    previousAngle = endTimeAngle;
                    break;
                default:
                    var previousPosition = currentTouch.PreviousLocationInView(this);
                    previousAngle = AngleBetween(previousPosition.ToMultivacPoint(), Center.ToMultivacPoint());
                    break;
            }

            var currentTapPosition = currentTouch.LocationInView(this);
            var currentAngle = AngleBetween(currentTapPosition.ToMultivacPoint(), Center.ToMultivacPoint());

            var angleChange = currentAngle - previousAngle;
            while (angleChange < -Math.PI) angleChange += FullCircle;
            while (angleChange > Math.PI) angleChange -= FullCircle;

            var timeChange = angleChange.AngleToTime();

            updateEditedTime(timeChange);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            finishTouchEditing();
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            if (currentTouch == null || currentTouch.Phase == UITouchPhase.Ended)
            {
                finishTouchEditing();
            }
        }

        private UITouch findValidTouch(NSSet touches)
        {
            foreach (UITouch touch in touches)
            {
                var position = touch.LocationInView(this);
                if (isCloseEnough(position, startTimePosition))
                {
                    updateType = WheelUpdateType.EditStartTime;
                    return touch;
                }

                if (IsRunning == false && isCloseEnough(position, endTimePosition))
                {
                    updateType = WheelUpdateType.EditEndTime;
                    return touch;
                }

                if (isOnTheWheelBetweenStartAndStop(position))
                {
                    updateType = WheelUpdateType.EditBothAtOnce;
                    return touch;
                }
            }

            return null;
        }

        private bool isCloseEnough(CGPoint tapPosition, CGPoint endPoint)
            => DistanceSq(tapPosition.ToMultivacPoint(), endPoint.ToMultivacPoint()) <= (Thickness / 2f) * (Thickness / 2f);

        private bool isOnTheWheelBetweenStartAndStop(CGPoint point)
        {
            var distanceFromCenterSq = DistanceSq(Center.ToMultivacPoint(), point.ToMultivacPoint());

            if (distanceFromCenterSq < SmallRadius * SmallRadius
                || distanceFromCenterSq > Radius * Radius)
            {
                return false;
            }

            var angle = AngleBetween(point.ToMultivacPoint(), Center.ToMultivacPoint());
            return isFullCircle
                ? true
                : angle.IsBetween(startTimeAngle, endTimeAngle);
        }

        private void updateEditedTime(TimeSpan diff)
        {
            var giveFeedback = false;

            if (updateType == WheelUpdateType.EditStartTime
                || updateType == WheelUpdateType.EditBothAtOnce)
            {
                giveFeedback = shouldGiveFeedback(startTime, startTime + diff);
                StartTime = startTime + diff;
            }

            if (IsRunning == false
                && (updateType == WheelUpdateType.EditEndTime || updateType == WheelUpdateType.EditBothAtOnce))
            {
                giveFeedback = giveFeedback || shouldGiveFeedback(endTime, endTime + diff);
                EndTime = endTime + diff;
            }

            if (giveFeedback)
            {
                feedbackGenerator.SelectionChanged();
                feedbackGenerator.Prepare();
            }
        }

        private bool shouldGiveFeedback(DateTimeOffset a, DateTimeOffset b)
        {
            if (a > b)
                (a, b) = (b, a);

            var minutes = a.Minute;
            var minutesDifference = Math.Abs(a.Minute - b.Minute);
            var minutesToTest = Math.Min(minutesDifference, feedbackEveryMinutes);
            for (var i = 1; i <= minutesToTest; i++)
            {
                if ((minutes + i) % feedbackEveryMinutes == 0)
                    return true;
            }

            return false;
        }

        private void finishTouchEditing()
        {
            currentTouch = null;
            feedbackGenerator = null;
        }

        private DateTimeOffset roundToLowerMinute(DateTimeOffset time)
            => time - TimeSpan.FromSeconds(time.Second);

        #endregion

        #region Shape layers factories

        private CALayer createBackgroundLayer()
        {
            var capArcRadius = Thickness / 2f;

            var startAngle = (nfloat)roundedStartTimeAngle;
            var endAngle = (nfloat)roundedEndTimeAngle;

            // these angles become obvious when you draw a diagram and mark all the angles
            var startCapStartAngle = startAngle + (nfloat)Math.PI;
            var startCapEndAngle = startAngle;
            var endCapStartAngle = endAngle;
            var endCapEndAngle = endAngle + (nfloat)Math.PI;

            var durationArc = new UIBezierPath();

            durationArc.AddArc(roundedStartTimePosition, capArcRadius, startCapStartAngle, startCapEndAngle, true); // start cap
            durationArc.AddArc(Center, Radius, startAngle, endAngle, true); // outer arc
            durationArc.AddArc(roundedEndTimePosition, capArcRadius, endCapStartAngle, endCapEndAngle, true); // end cap
            durationArc.AddArc(Center, SmallRadius, endAngle, startAngle, false); // inner arc

            var layer = new CAShapeLayer();
            layer.Path = durationArc.CGPath;
            layer.FillColor = backgroundColor;

            if (isFullCircle)
            {
                // cap shadows
                var shadowPath = new UIBezierPath();
                shadowPath.AddArc(roundedStartTimePosition, capArcRadius, 0f, (nfloat)FullCircle, false);
                shadowPath.AddArc(roundedEndTimePosition, capArcRadius, 0f, (nfloat)FullCircle, true);

                layer.ShadowPath = shadowPath.CGPath;
                layer.ShadowColor = shadowColor;
                layer.ShadowRadius = Resize(shadowRadius);
                layer.ShadowOpacity = shadowOpacity;
                layer.ShadowOffset = CGSize.Empty;

                var wheelLayer = CreateWheelLayer(backgroundColor);
                layer.Mask = wheelLayer;
            }

            return layer;
        }

        private CALayer createCap(CGPoint center)
        {
            var innerRadius = Resize(capRadius);
            var outerRadius = (Radius - SmallRadius) / 2;
            var centerDistance = SmallRadius + outerRadius;

            var outerPath = new UIBezierPath();
            outerPath.AddArc(center, outerRadius, 0, (nfloat)FullCircle, false);

            var backgroundLayer = new CAShapeLayer();
            backgroundLayer.Path = outerPath.CGPath;
            backgroundLayer.FillColor = backgroundColor;

            var innerPath = new UIBezierPath();
            innerPath.AddArc(center, innerRadius, 0, (nfloat)FullCircle, false);

            // two horizontal bars
            var length = Resize(horizontalBarLength);
            var height = Resize(horizontalBarHeight);
            var offset = Resize(horizontalBarsDistance / 2);
            var roundedCornersRadius = Resize(horizontalBarCornerRadius);

            var topHorizontalBar = UIBezierPath.FromRoundedRect(
                new CGRect(center.X - length / 2, center.Y - height - offset, length, height),
                UIRectCorner.AllCorners,
                new CGSize(roundedCornersRadius, roundedCornersRadius));
            var topBar = new CAShapeLayer();
            topBar.Path = topHorizontalBar.CGPath;
            topBar.FillColor = backgroundColor;

            var bottomHorizontalBar = UIBezierPath.FromRoundedRect(
                new CGRect(center.X - length / 2, center.Y + offset, length, height),
                UIRectCorner.AllCorners,
                new CGSize(roundedCornersRadius, roundedCornersRadius));
            var bottomBar = new CAShapeLayer();
            bottomBar.Path = bottomHorizontalBar.CGPath;
            bottomBar.FillColor = backgroundColor;

            // combine layers
            var circleLayer = new CAShapeLayer();
            circleLayer.Path = innerPath.CGPath;
            circleLayer.FillColor = capColor;

            circleLayer.AddSublayer(topBar);
            circleLayer.AddSublayer(bottomBar);

            backgroundLayer.AddSublayer(circleLayer);

            return backgroundLayer;
        }

        #endregion

        private enum WheelUpdateType
        {
            EditStartTime,
            EditEndTime,
            EditBothAtOnce
        }
    }
}

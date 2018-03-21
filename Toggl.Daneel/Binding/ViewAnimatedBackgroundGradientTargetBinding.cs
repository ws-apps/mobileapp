using CoreAnimation;
using CoreGraphics;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ViewAnimatedBackgroundGradientTargetBinding : MvxTargetBinding<UIView, UIColor>
    {
        public const string BindingName = "AnimatedBackgroundGradient";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        private CAGradientLayer gradientLayer;

        public ViewAnimatedBackgroundGradientTargetBinding(UIView target)
            : base(target)
        {
            gradientLayer = new CAGradientLayer();
            gradientLayer.StartPoint = new CGPoint(0.5, 0);
            gradientLayer.EndPoint = new CGPoint(0.5, 1);
            gradientLayer.Frame = target.Frame;

            target.Layer.InsertSublayer(gradientLayer, 0);
        }

        protected override void SetValue(UIColor value)
        {
            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.SharpCurve,
                () => gradientLayer.Colors = new[] { UIColor.White.CGColor, value.CGColor }
            );
        }
    }
}

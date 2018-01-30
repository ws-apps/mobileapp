using CoreAnimation;
using CoreGraphics;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ViewAnimatedVisibilityTargetBinding : MvxTargetBinding<UIView, bool>
    {
        public const string BindingName = "AnimatedVisibility";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public ViewAnimatedVisibilityTargetBinding(UIView target)
            : base(target)
        {
        }

        protected override void SetValue(bool value)
        {
            if (Target.Hidden != value) return;

            Target.Transform = CGAffineTransform.MakeTranslation(0, 20);

            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.SharpCurve,
                () => 
                {
                    Target.Hidden = !value;
                    Target.Transform = CGAffineTransform.MakeTranslation(0, 0);
                }
            );
        }
    }
}

using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ImageViewImageTargetBinding : MvxTargetBinding<UIImageView, UIImage>
    {
        public const string BindingName = "AnimatedImage";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public ImageViewImageTargetBinding(UIImageView target)
            : base(target)
        {
        }

        protected override void SetValue(UIImage value)
        {
            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.SharpCurve,
                () => Target.Image = value
            );
        }
    }
}

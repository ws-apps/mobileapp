using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;

namespace Toggl.Giskard.Bindings
{
    public sealed class ImageViewVerticalFlipTargetBinding : MvxAndroidTargetBinding<ImageView, bool>
    {
        public const string BindingName = "VerticalFlip";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ImageViewVerticalFlipTargetBinding(ImageView target) 
            : base(target)
        {
        }

        protected override void SetValueImpl(ImageView target, bool value)
            => target.Animate().RotationX(value ? 180 : 0).Start();
    }
}
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class ButtonImageTargetBinding : MvxTargetBinding<UIButton, UIImage>
    {
        public const string BindingName = "Image";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public ButtonImageTargetBinding(UIButton target)
            : base(target) 
        {
        }

        protected override void SetValue(UIImage value)
            => Target.SetImage(value, UIControlState.Normal);
    }
}

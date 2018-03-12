using Android.Graphics;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;

namespace Toggl.Giskard.Bindings
{
    public sealed class TextViewFontWeightTargetBinding : MvxTargetBinding<TextView, TypefaceStyle>
    {
        public const string BindingName = "FontWeight";

        public override MvxBindingMode DefaultMode => MvxBindingMode.Default;

        public TextViewFontWeightTargetBinding(TextView target)
                : base(target)
        {
        }

        protected override void SetValue(TypefaceStyle value)
        {
            Target.Typeface = Typeface.DefaultFromStyle(value);
        }
    }
}

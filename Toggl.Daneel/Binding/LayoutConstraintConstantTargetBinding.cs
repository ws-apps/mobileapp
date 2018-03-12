using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class LayoutConstraintConstantTargetBinding
        : MvxTargetBinding<NSLayoutConstraint, nfloat>
    {
        public const string BindingName = "Constant";

        public LayoutConstraintConstantTargetBinding(NSLayoutConstraint target)
            : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(nfloat value)
        {
            Target.Constant = value;
            var targetView = (UIView)Target.FirstItem;
            targetView.SetNeedsLayout();
            targetView.LayoutIfNeeded();
        }
    }
}

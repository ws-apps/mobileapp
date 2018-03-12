using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Daneel.Views;

namespace Toggl.Daneel.Binding
{
    public class SpiderOnARopeViewIsVisibleTargetBinding : MvxTargetBinding<SpiderOnARopeView, bool>
    {
        public const string BindingName = "SpiderVisibility";

        public SpiderOnARopeViewIsVisibleTargetBinding(SpiderOnARopeView target) : base(target)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValue(bool value)
        {
            if (Target.IsVisible == value) return;

            if (value)
            {
                Target.Show();
            }
            else
            {
                Target.Hide();
            }
        }
    }
}

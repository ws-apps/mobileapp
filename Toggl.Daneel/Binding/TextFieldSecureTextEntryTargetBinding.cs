using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class TextFieldSecureTextEntryTargetBinding : MvxTargetBinding<UITextField, bool>
    {
        public const string BindingName = "SecureTextEntry";

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public TextFieldSecureTextEntryTargetBinding(UITextField target)
            : base(target)
        {
            target.EditingDidBegin += onEditingDidBegin;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;
            Target.EditingDidBegin -= onEditingDidBegin;
        }

        protected override void SetValue(bool value)
        {
            Target.ResignFirstResponder();
            Target.SecureTextEntry = value;
            Target.BecomeFirstResponder();
        }

        private void onEditingDidBegin(object sender, EventArgs e)
        {
            if (!Target.SecureTextEntry) return;
            Target.InsertText(Target.Text);
        }
    }
}

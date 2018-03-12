using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.MvvmCross.Autocomplete;
using Toggl.Giskard.Autocomplete;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Bindings
{
    public sealed class EditTextTextFieldInfoTargetBinding : BaseTextFieldInfoTargetBinding<EditText>
    {
        public const string BindingName = "TextFieldInfo";

        private readonly AutocompleteTextWatcher spanWatcher = new AutocompleteTextWatcher();

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override IAutocompleteEventProvider EventProvider => spanWatcher;

        public EditTextTextFieldInfoTargetBinding(EditText target)
                : base(target)
        {
            Target.AddTextChangedListener(spanWatcher);
        }

        protected override string GetCurrentTimeEntryDescription()
            => Target.GetDescription();

        protected override int GetCurrentCursorPosition()
            => Target.SelectionStart;

        protected override bool CheckIfSelectingText()
            => Target.SelectionStart != Target.SelectionEnd;

        protected override void UpdateTarget(TextFieldInfo textFieldInfo)
        {
            if (!Target.IsFocused) return;

            var formattedText = textFieldInfo.GetSpannableText();

            if (Target.TextFormatted.Length() == formattedText.Length())
                return;

            Target.TextFormatted = formattedText;
            Target.SetSelection(textFieldInfo.CursorPosition);
        }

        protected override void MarkViewForRedrawing()
        {
            Target.Invalidate();
        }
    }
}

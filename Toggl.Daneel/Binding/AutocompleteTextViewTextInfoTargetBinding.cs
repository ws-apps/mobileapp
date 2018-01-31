using System;
using Foundation;
using Toggl.Daneel.Autocomplete;
using Toggl.Daneel.Views;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.MvvmCross.Autocomplete;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class AutocompleteTextViewTextInfoTargetBinding : BaseTextFieldInfoTargetBinding<AutocompleteTextView>
    {
        public const string BindingName = "TextFieldInfo";
        private const string selectedTextRangeChangedKey = "selectedTextRange";

        private readonly IDisposable selectedTextRangeDisposable;

        private TextFieldInfo textFieldInfo = TextFieldInfo.Empty;

        protected override IAutocompleteEventProvider EventProvider { get; }

        public AutocompleteTextViewTextInfoTargetBinding(AutocompleteTextView target)
            : base(target)
        {
            EventProvider = target.AutocompleteTextViewInfoDelegate;
            Target.Delegate = target.AutocompleteTextViewInfoDelegate;

            selectedTextRangeDisposable = Target.AddObserver(
                selectedTextRangeChangedKey,
                NSKeyValueObservingOptions.OldNew,
                _ => target.AutocompleteTextViewInfoDelegate.RaisePositionChanged()
            );
        }

        protected override string GetCurrentTimeEntryDescription()
            => Target.GetDescription();

        protected override int GetCurrentCursorPosition()
            => (int)Target.SelectedRange.Location;

        protected override bool CheckIfSelectingText()
            => Target.SelectedRange.Length > 0;

        protected override void UpdateTarget(TextFieldInfo textFieldInfo)
        {
            if (!Target.IsFirstResponder || Target.BeginningOfDocument == null)
                return;

            Target.AttributedText = textFieldInfo.GetAttributedText();

            var positionToSet = Target.GetPosition(Target.BeginningOfDocument, textFieldInfo.CursorPosition);
            Target.SelectedTextRange = Target.GetTextRange(positionToSet, positionToSet);
        }

        protected override void MarkViewForRedrawing()
        {
            Target.SetNeedsDisplay();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            selectedTextRangeDisposable?.Dispose();
        }
    }
}

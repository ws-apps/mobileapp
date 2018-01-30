using System;
using Foundation;
using Toggl.Daneel.Autocomplete;
using Toggl.Daneel.Views;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.MvvmCross.Autocomplete;
using UIKit;

namespace Toggl.Daneel.Binding
{
    public sealed class TextViewTextInfoTargetBinding : BaseTextFieldInfoTargetBinding<UITextView>
    {
        public const string BindingName = "TextFieldInfo";
        private const string selectedTextRangeChangedKey = "selectedTextRange";

        private readonly TextViewPlaceholderManager placeholder;
        private readonly IDisposable selectedTextRangeDisposable;
        private readonly AutocompleteTextViewDelegate textViewInfoDelegate = new AutocompleteTextViewDelegate();

        private TextFieldInfo textFieldInfo = TextFieldInfo.Empty;

        protected override IAutocompleteEventProvider EventProvider { get; }

        public TextViewTextInfoTargetBinding(UITextView target)
            : base(target)
        {
            EventProvider = textViewInfoDelegate;
            Target.Delegate = textViewInfoDelegate;
           
            placeholder = new TextViewPlaceholderManager(target);
            textViewInfoDelegate.TextViewDidChange += placeholder.UpdateVisibility;
            selectedTextRangeDisposable = Target.AddObserver(
                selectedTextRangeChangedKey,
                NSKeyValueObservingOptions.OldNew,
                _ => textViewInfoDelegate.RaisePositionChanged()
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

            placeholder.UpdateVisibility();
        }

        protected override void MarkViewForRedrawing()
        {
            Target.SetNeedsDisplay();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            Target.Delegate = null;
            selectedTextRangeDisposable?.Dispose();
            textViewInfoDelegate.TextViewDidChange -= placeholder.UpdateVisibility;
        }
    }
}

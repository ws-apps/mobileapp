using System;
using System.Globalization;
using Foundation;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.Core;
using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using UIKit;
using CoreText;
using Toggl.Foundation.MvvmCross.Helper;
using MvvmCross.Plugins.Color.iOS;

namespace Toggl.Daneel.Views.EditDuration
{
    [Register(nameof(DurationField))]
    public sealed class DurationField : UITextField
    {
        private TimeSpan originalDuration;

        private TimeSpan duration;

        private bool isEditing;

        private DurationInputDelegate durationInputDelegate;

        private DurationFieldInfo input;

        private CTStringAttributes noAttributes;

        public MvxValueConverter<TimeSpan, string> Converter { get; set; }

        public event EventHandler DurationChanged;

        public TimeSpan Duration
        {
            get => duration;
            set
            {
                duration = value;
                if (isEditing == false)
                    showCurrentDuration();
            }
        }

        public DurationField(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            KeyboardType = UIKeyboardType.NumberPad;
            AdjustsFontSizeToFitWidth = false;
            Font = Font.GetMonospacedDigitFont();
            TintColor = Color.DurationField.Cursor.ToNativeColor();

            durationInputDelegate = new DurationInputDelegate();
            Delegate = durationInputDelegate;

            durationInputDelegate.BackspacePressed += backspacePressed;
            durationInputDelegate.NumberKeyPressed += numberKeyPressed;
            durationInputDelegate.StartEditing += startEditing;
            durationInputDelegate.FinishEditing += finishEditing;

            noAttributes = new CTStringAttributes();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == false) return;

            Delegate = null;

            durationInputDelegate.BackspacePressed -= backspacePressed;
            durationInputDelegate.NumberKeyPressed -= numberKeyPressed;
            durationInputDelegate.StartEditing -= startEditing;
            durationInputDelegate.FinishEditing -= finishEditing;
        }

        private void backspacePressed(object sender, EventArgs e)
        {
            var nextInput = input.Pop();
            tryUpdate(nextInput);
        }

        private void numberKeyPressed(object sender, NumberKeyPressedEventArgs e)
        {
            var nextInput = input.Push(e.Number);
            tryUpdate(nextInput);
        }

        private void startEditing(object sender, EventArgs e)
        {
            isEditing = true;
            originalDuration = duration;
            input = DurationFieldInfo.Empty;
            setText(input.ToString());
        }

        private void finishEditing(object sender, EventArgs e)
        {
            isEditing = false;
            showCurrentDuration();
        }

        private void tryUpdate(DurationFieldInfo nextInput)
        {
            if (nextInput.Equals(input) == false)
            {
                input = nextInput;

                Duration = input.IsEmpty
                    ? originalDuration
                    : input.ToTimeSpan();

                DurationChanged.Raise(this);
                setText(input.ToString());
            }
        }

        private void showCurrentDuration()
        {
            var text = (string)Converter?.Convert(duration, typeof(TimeSpan), null, CultureInfo.CurrentCulture) ?? String.Empty;
            setText(text);
        }

        private void setText(string text)
        {
            AttributedText = new NSAttributedString(text, noAttributes);
        }

        private class DurationInputDelegate : UITextFieldDelegate
        {
            public event EventHandler StartEditing;
            public event EventHandler FinishEditing;
            public event EventHandler BackspacePressed;
            public event EventHandler<NumberKeyPressedEventArgs> NumberKeyPressed;

            public override void EditingStarted(UITextField textField)
            {
                StartEditing.Raise(this);
            }

            public override void EditingEnded(UITextField textField, UITextFieldDidEndEditingReason reason)
            {
                FinishEditing.Raise(this);
            }

            public override bool ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
            {
                if (replacementString.Length > 1) return false;

                if (isPressingBackspace(range, replacementString))
                {
                    BackspacePressed.Raise(this);
                }
                else if (int.TryParse(replacementString, out var number) && number >= 0 && number <= 9)
                {
                    NumberKeyPressed?.Invoke(this, new NumberKeyPressedEventArgs(number));
                }

                // never update the text automaticaly
                return false;
            }

            private static bool isPressingBackspace(NSRange range, string text)
                => range.Length == 1 && text.Length == 0;
        }
    }
}

using System;
using Foundation;
using Toggl.Daneel.Autocomplete;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(AutocompleteTextView))]
    public sealed class AutocompleteTextView : UITextView
    {
        public AutocompleteTextViewDelegate AutocompleteTextViewInfoDelegate { get; } = new AutocompleteTextViewDelegate();
        
        public AutocompleteTextView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Delegate = AutocompleteTextViewInfoDelegate;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            Delegate = null;
        }
    }
}

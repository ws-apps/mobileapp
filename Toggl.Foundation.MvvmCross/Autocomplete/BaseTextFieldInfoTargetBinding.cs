using System;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Foundation.Autocomplete;

namespace Toggl.Foundation.MvvmCross.Autocomplete
{
    public abstract class BaseTextFieldInfoTargetBinding<TView> : MvxTargetBinding<TView, TextFieldInfo>
        where TView : class
    {
        private bool isSelectingText;

        protected TextFieldInfo TextFieldInfo { get; set; }

        protected abstract IAutocompleteEventProvider EventProvider { get; }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected BaseTextFieldInfoTargetBinding(TView target)
            : base(target)
        {
        }

        protected abstract string GetCurrentTimeEntryDescription();

        protected abstract int GetCurrentCursorPosition();

        protected abstract bool CheckIfSelectingText();

        protected abstract void UpdateTarget(TextFieldInfo textFieldInfo);

        protected abstract void MarkViewForRedrawing();

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            EventProvider.TagDeleted += onTagDeleted;
            EventProvider.TextChanged += onTextChanged;
            EventProvider.ProjectDeleted += onProjectDeleted;
            EventProvider.CursorPositionChanged += onCursorPositionChanged;
        }

        protected sealed override void SetValue(TextFieldInfo value)
        {
            setTextFieldInfo(value);

            if (isSelectingText) return;

            UpdateTarget(value);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (!isDisposing) return;

            EventProvider.TagDeleted -= onTagDeleted;
            EventProvider.TextChanged -= onTextChanged;
            EventProvider.ProjectDeleted -= onProjectDeleted;
            EventProvider.CursorPositionChanged -= onCursorPositionChanged;
        }

        private void onTextChanged(object sender, EventArgs e)
            => triggerValueChanged();

        private void onCursorPositionChanged(object sender, EventArgs e)
            => triggerValueChanged();

        private void onProjectDeleted(object sender, EventArgs e)
        {
            onTextFieldInfoChanged(
                TextFieldInfo
                    .RemoveProjectInfo()
                    .WithTextAndCursor(TextFieldInfo.Text, TextFieldInfo.Text.Length));
        }

        private void onTagDeleted(object sender, TagDeletedEventArgs e)
        {
            onTextFieldInfoChanged(
                TextFieldInfo
                    .RemoveTag(TextFieldInfo.Tags[e.TagIndex])
                    .WithTextAndCursor(TextFieldInfo.Text, e.CursorPosition));
        }

        private void triggerValueChanged()
        {
            isSelectingText = CheckIfSelectingText();
            var reportedCursorPosition = GetCurrentCursorPosition();

            var newDescription = GetCurrentTimeEntryDescription();
            var descriptionLength = newDescription.Length;

            var isEditingInTokenRegion = newDescription != TextFieldInfo.Text && reportedCursorPosition > newDescription.Length;
            var newCursorPosition = isEditingInTokenRegion ? newDescription.Length : reportedCursorPosition;

            onTextFieldInfoChanged(TextFieldInfo.WithTextAndCursor(newDescription, newCursorPosition));
        }

        private void onTextFieldInfoChanged(TextFieldInfo info)
        {
            setTextFieldInfo(info);
            FireValueChanged(info);
        }

        private void setTextFieldInfo(TextFieldInfo info)
        {
            if (TextFieldInfo.Text != info.Text
                || TextFieldInfo.ProjectName != info.ProjectName
                || TextFieldInfo.Tags.Length != info.Tags.Length)
                MarkViewForRedrawing();

            TextFieldInfo = info;
        }
    }
}

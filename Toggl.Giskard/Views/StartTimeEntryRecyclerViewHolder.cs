using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.Autocomplete.Suggestions;

namespace Toggl.Giskard.Views
{
    public sealed class StartTimeEntryRecyclerViewHolder : MvxRecyclerViewHolder
    {
        private Button toggleTasksButton;
        private bool continueClickOverloaded;

        private IMvxCommand<ProjectSuggestion> toggleTasksCommand;
        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand
        {
            get => toggleTasksCommand;
            set
            {
                if (toggleTasksCommand == value) return;

                toggleTasksCommand = value;
                if (toggleTasksCommand == null) return;

                ensureContinueClickOverloaded();
            }
        }

        public StartTimeEntryRecyclerViewHolder(View itemView, IMvxAndroidBindingContext context)
            : base(itemView, context)
        {
        }

        public StartTimeEntryRecyclerViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        private void ensureContinueClickOverloaded()
        {
            if (continueClickOverloaded) return;
            toggleTasksButton = ItemView.FindViewById<Button>(Resource.Id.StartTimeEntryToggleTasksButton);
            toggleTasksButton.Click += onContinueButtonClick;
            continueClickOverloaded = true;
        }

        private void onContinueButtonClick(object sender, EventArgs e)
        {
            ExecuteCommandOnItem(ToggleTasksCommand);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            if (toggleTasksButton == null) return;
            toggleTasksButton.Click -= onContinueButtonClick;
        }
    }
}

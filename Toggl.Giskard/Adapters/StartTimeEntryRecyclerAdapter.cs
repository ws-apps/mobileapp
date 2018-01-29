using System;
using System.Linq;
using Android.Runtime;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;

namespace Toggl.Giskard.Adapters
{
    public sealed class StartTimeEntryRecyclerAdapter
        : SegmentedRecyclerAdapter<WorkspaceGroupedCollection<AutocompleteSuggestion>, AutocompleteSuggestion>
    {
        public bool UseGrouping { get; set; }

        public StartTimeEntryRecyclerAdapter()
        {
        }

        public StartTimeEntryRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int ItemCount
            => UseGrouping ? base.ItemCount : Collection.FirstOrDefault()?.Count ?? 0;

        public override object GetItem(int viewPosition)
            => UseGrouping ? base.GetItem(viewPosition) : Collection.First()[viewPosition];

        protected override MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>> Collection
            => ItemsSource as MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>>;
    }
}

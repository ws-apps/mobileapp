using System;
using System.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Giskard.TemplateSelectors;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Adapters
{
    public sealed class StartTimeEntryRecyclerAdapter
        : SegmentedRecyclerAdapter<WorkspaceGroupedCollection<AutocompleteSuggestion>, AutocompleteSuggestion>
    {
        public bool UseGrouping { get; set; }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType != StartTimeEntrySuggestionsTemplateSelector.ProjectSuggestion)
                return base.OnCreateViewHolder(parent, viewType);

            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new StartTimeEntryRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = ItemClick,
                LongClick = ItemLongClick,
                ToggleTasksCommand = ToggleTasksCommand
            };

            return viewHolder;
        }
    }
}

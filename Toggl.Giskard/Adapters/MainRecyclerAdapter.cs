using System;
using System.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.TemplateSelectors;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Adapters
{
    public sealed class MainRecyclerAdapter 
        : SegmentedRecyclerAdapter<TimeEntryViewModelCollection, TimeEntryViewModel>
    {
        public bool ShouldShowSuggestions
            => SuggestionsViewModel?.Suggestions.Any() ?? false;

        public SuggestionsViewModel SuggestionsViewModel { get; set; }

        public TimeEntriesLogViewModel TimeEntriesLogViewModel { get; set; }

        public MainRecyclerAdapter()
        {
        }

        public MainRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override int HeaderOffsetForAnimation => ShouldShowSuggestions ? 1 : 0;

        protected override MvxObservableCollection<TimeEntryViewModelCollection> Collection
            => ItemsSource as MvxObservableCollection<TimeEntryViewModelCollection>;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);

            switch (viewType)
            {
                case MainTemplateSelector.Item:
                    return new MainRecyclerViewLogViewHolder(inflatedView, itemBindingContext)
                    {
                        Click = TimeEntriesLogViewModel.EditCommand,
                        ContinueCommand = TimeEntriesLogViewModel.ContinueTimeEntryCommand
                    };

                case MainTemplateSelector.Suggestions:
                    return new MainRecyclerViewSuggestionsViewHolder(inflatedView, itemBindingContext)
                    {
                        Click = SuggestionsViewModel.StartTimeEntryCommand
                    };

                default:
                    return new MvxRecyclerViewHolder(inflatedView, itemBindingContext);
            }

            throw new ArgumentOutOfRangeException(nameof(viewType), $"Invalid viewType provided to {nameof(MainRecyclerAdapter)}");
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            if (holder is MainRecyclerViewLogViewHolder timeEntriesLogRecyclerViewHolder
                && GetItem(position) is TimeEntryViewModel timeEntry)
            {
                timeEntriesLogRecyclerViewHolder.CanSync = timeEntry.CanSync;
            }
        }

        public override int ItemCount => base.ItemCount + 1 + (ShouldShowSuggestions ? 1 : 0);

        public override object GetItem(int viewPosition)
        {
            if (viewPosition == 0 && ShouldShowSuggestions)
                return SuggestionsViewModel;
            
            if (viewPosition == ItemCount - 1)
                return null;

            return base.GetItem(viewPosition - (ShouldShowSuggestions ? 1 : 0));
        }

        internal void ContinueTimeEntry(int viewPosition)
        {
            var timeEntry = GetItem(viewPosition) as TimeEntryViewModel;
            if (timeEntry == null) return;
            TimeEntriesLogViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntry);
        }
    }
}

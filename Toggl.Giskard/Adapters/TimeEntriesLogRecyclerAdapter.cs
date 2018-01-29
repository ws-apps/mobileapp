using System;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.TemplateSelectors;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Adapters
{
    public sealed class TimeEntriesLogRecyclerAdapter 
        : SegmentedRecyclerAdapter<TimeEntryViewModelCollection, TimeEntryViewModel>
    {
        public IMvxAsyncCommand<TimeEntryViewModel> ContinueCommand { get; set; }

        public TimeEntriesLogRecyclerAdapter()
        {
        }

        public TimeEntriesLogRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override MvxObservableCollection<TimeEntryViewModelCollection> Collection
            => ItemsSource as MvxObservableCollection<TimeEntryViewModelCollection>;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType != TimeEntriesLogTemplateSelector.Item)
                return base.OnCreateViewHolder(parent, viewType);

            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new TimeEntriesLogRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = ItemClick,
                LongClick = ItemLongClick,
                ContinueCommand = ContinueCommand
            };

            return viewHolder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            base.OnBindViewHolder(holder, position);

            if (holder is TimeEntriesLogRecyclerViewHolder timeEntriesLogRecyclerViewHolder
                && GetItem(position) is TimeEntryViewModel timeEntry)
            {
                timeEntriesLogRecyclerViewHolder.CanSync = timeEntry.CanSync;
            }
        }

        public override int ItemCount => base.ItemCount + 1;

        public override object GetItem(int viewPosition)
        {
            if (viewPosition == ItemCount - 1)
                return null;

            return base.GetItem(viewPosition);
        }

        internal void ContinueTimeEntry(int viewPosition)
        {
            var timeEntry = GetItem(viewPosition) as TimeEntryViewModel;
            if (timeEntry == null) return;
            ContinueCommand?.ExecuteAsync(timeEntry);
        }
    }
}

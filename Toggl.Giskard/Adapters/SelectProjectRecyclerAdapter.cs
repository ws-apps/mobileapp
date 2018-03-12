using System;
using System.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Giskard.TemplateSelectors;
using Toggl.Giskard.Views;

namespace Toggl.Giskard.Adapters
{
    public sealed class SelectProjectRecyclerAdapter :
        CreateSuggestionGroupedTableViewSource<WorkspaceGroupedCollection<AutocompleteSuggestion>, AutocompleteSuggestion>
    {
        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

        public bool UseGrouping { get; set; }

        protected override MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>> Collection
            => ItemsSource as MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>>;

        public SelectProjectRecyclerAdapter()
        {
        }

        public SelectProjectRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int ItemCount
        {
            get
            {
                if (Collection.Count == 0)
                    return base.ItemCount;

                return base.ItemCount - (UseGrouping ? 0 : 1);
            }
        }

        public override object GetItem(int viewPosition)
        {
            if (UseGrouping)
                return base.GetItem(viewPosition);

            if (IsSuggestingCreation && viewPosition == 0)
                return GetCreateSuggestionItem();

            var actualPosition = viewPosition - (IsSuggestingCreation ? 1 : 0);
            return Collection.First()[actualPosition];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType != SelectProjectTemplateSelector.ProjectSuggestion)
                return base.OnCreateViewHolder(parent, viewType);

            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);

            var viewHolder = new SelectProjectWithExpandableTasksRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = ItemClick,
                LongClick = ItemLongClick,
                ToggleTasksCommand = ToggleTasksCommand,
                ToggleTasksExpansionButtonId = Resource.Id.SelectProjectToggleTasksButton
            };

            return viewHolder;
        }

        protected override int SuggestCreationViewType
            => SelectProjectTemplateSelector.CreateEntity;

        protected override object GetCreateSuggestionItem()
            => $"{Resources.CreateProject} \"{Text}\"";
    }

}
using System;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.TemplateSelectors;

namespace Toggl.Giskard.Adapters
{
    public sealed class SelectClientRecyclerAdapter : MvxRecyclerAdapter
    {
        public string Text { get; set; }

        public IMvxAsyncCommand CreateCommand { get; set; }

        private bool isSuggestingCreation;
        public bool IsSuggestingCreation
        {
            get => isSuggestingCreation;
            set
            {
                if (isSuggestingCreation == value) return;
                isSuggestingCreation = value;

                if (isSuggestingCreation)
                {
                    NotifyItemInserted(0);
                }
                else
                {
                    NotifyItemRemoved(0);
                }
            }
        }

        public SelectClientRecyclerAdapter()
        {
        }

        public SelectClientRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int ItemCount
            => base.ItemCount + (IsSuggestingCreation ? 1 : 0);

        public override object GetItem(int viewPosition)
        {
            if (IsSuggestingCreation && viewPosition == 0)
                return $"Create client \"{Text.Trim()}\"";

            var actualViewPosition = viewPosition - (IsSuggestingCreation ? 1 : 0);
            return base.GetItem(actualViewPosition);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemBindingContext = new MvxAndroidBindingContext(parent.Context, BindingContext.LayoutInflaterHolder);
            var inflatedView = InflateViewForHolder(parent, viewType, itemBindingContext);
            var viewHolder = new MvxRecyclerViewHolder(inflatedView, itemBindingContext)
            {
                Click = viewType == SelectClientTemplateSelector.CreateEntity ? CreateCommand : ItemClick,
                LongClick = ItemLongClick,
            };

            return viewHolder;
        }
    }
}

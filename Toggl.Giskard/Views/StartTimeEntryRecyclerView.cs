using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.StartTimeEntryRecyclerView")]
    public sealed class StartTimeEntryRecyclerView : MvxRecyclerView
    {
        public StartTimeEntryRecyclerAdapter StartTimeEntryRecyclerAdapter => Adapter as StartTimeEntryRecyclerAdapter;

        public bool UseGrouping
        {
            get => StartTimeEntryRecyclerAdapter.UseGrouping;
            set => StartTimeEntryRecyclerAdapter.UseGrouping = value;
        }

        public StartTimeEntryRecyclerView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public StartTimeEntryRecyclerView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public StartTimeEntryRecyclerView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle, new StartTimeEntryRecyclerAdapter())
        {
            SetItemViewCacheSize(20);
            DrawingCacheEnabled = true;
            DrawingCacheQuality = DrawingCacheQuality.High;
        }
    }
}

using System;
using System.Windows.Input;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.Adapters
{
    public sealed class ReportsRecyclerAdapter : MvxRecyclerAdapter
    {
        public ReportsViewModel ViewModel { get; set; }

        public ReportsRecyclerAdapter()
        {
        }

        public ReportsRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override int ItemCount => 1;

        public override object GetItem(int viewPosition) => ViewModel;
    }
}

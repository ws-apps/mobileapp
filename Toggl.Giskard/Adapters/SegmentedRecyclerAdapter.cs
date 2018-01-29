using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Android.Runtime;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;

namespace Toggl.Giskard.Adapters
{
    public abstract class SegmentedRecyclerAdapter<TCollection, TItem> : MvxRecyclerAdapter
        where TCollection : MvxObservableCollection<TItem>
    {
        private readonly object headerListLock = new object();
        private readonly List<int> headerIndexes = new List<int>();

        private int? cachedItemCount;

        protected abstract MvxObservableCollection<TCollection> Collection { get; }

        public SegmentedRecyclerAdapter()
        {
        }

        public SegmentedRecyclerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override void SetItemsSource(IEnumerable value)
        {
            base.SetItemsSource(value);

            calculateHeaderIndexes();
        }

        protected override void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsSourceCollectionChanged(sender, e);

            calculateHeaderIndexes();
        }

        public override object GetItem(int viewPosition)
        {
            try
            {
                var item = tryGetItem(viewPosition);
                return item;
            }
            catch
            {
                calculateHeaderIndexes();
                var item = tryGetItem(viewPosition);
                return item;
            }
        }

        public override int ItemCount
        {
            get
            {
                if (cachedItemCount == null)
                {
                    var collection = Collection;
                    if (collection == null) return 0;

                    var itemCount = collection.Aggregate(collection.Count, (acc, g) => acc + g.Count);
                    cachedItemCount = itemCount;
                }

                return cachedItemCount.Value;
            }
        }

        private void calculateHeaderIndexes()
        {
            lock (headerListLock)
            {
                cachedItemCount = null;
                headerIndexes.Clear();

                var collection = Collection;
                if (collection == null) return;

                var index = 0;
                foreach (var nestedCollection in collection)
                {
                    headerIndexes.Add(index);
                    index += nestedCollection.Count + 1;
                }
            }
        }

        private object tryGetItem(int viewPosition)
        {
            var collection = Collection;
            if (collection == null)
                return null;

            var groupIndex = headerIndexes.IndexOf(viewPosition);
            if (groupIndex >= 0)
                return collection[groupIndex];

            var currentGroupIndex = headerIndexes.FindLastIndex(index => index < viewPosition);
            var offset = headerIndexes[currentGroupIndex] + 1;
            var indexInGroup = viewPosition - offset;
            return collection[currentGroupIndex][indexInGroup];
        }
    }
}

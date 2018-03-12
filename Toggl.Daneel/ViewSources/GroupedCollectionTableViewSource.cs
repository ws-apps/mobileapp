using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Foundation;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Daneel.Views.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public abstract class GroupedCollectionTableViewSource<TCollection, TItem> : MvxTableViewSource
        where TCollection : MvxObservableCollection<TItem>, new()
        where TItem : class
    {
        private readonly string cellIdentifier;
        private readonly string headerCellIdentifier;
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        protected IEnumerable<MvxObservableCollection<TItem>> GroupedItems
            => ItemsSource as IEnumerable<MvxObservableCollection<TItem>>;

        protected NestableObservableCollection<TCollection, TItem> AnimatableCollection
            => ItemsSource as NestableObservableCollection<TCollection, TItem>;

        protected GroupedCollectionTableViewSource(UITableView tableView, string cellIdentifier, string headerCellIdentifier)
            : base(tableView)
        {
            this.cellIdentifier = cellIdentifier;
            this.headerCellIdentifier = headerCellIdentifier;

            UseAnimations = true;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var grouping = GetGroupAt(section);

            var cell = GetOrCreateHeaderViewFor(tableView);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = grouping;

            if (section == 0 && cell is IHeaderViewCellWithHideableTopSeparator headerCell)
                headerCell.TopSeparatorHidden = true;

            return cell;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);

            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public override nint NumberOfSections(UITableView tableView)
            => ItemsSource.Count();

        public override nint RowsInSection(UITableView tableview, nint section)
            => GetGroupAt(section).Count();

        protected virtual IEnumerable<TItem> GetGroupAt(nint section)
            => GroupedItems.ElementAtOrDefault((int)section) ?? new TCollection();

        protected override object GetItemAt(NSIndexPath indexPath)
            => GroupedItems.ElementAtOrDefault(indexPath.Section)?.ElementAtOrDefault((int)indexPath.Item);

        public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
        {
            var firstVisible = TableView.IndexPathsForVisibleRows.First();
            if (firstVisible.Section != section + 1) return;

            var nextHeader = TableView.GetHeaderView(firstVisible.Section) as IHeaderViewCellWithHideableTopSeparator;
            if (nextHeader == null) return;

            nextHeader.TopSeparatorHidden = true;
        }

        public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
        {
            var headerViewCell = headerView as IHeaderViewCellWithHideableTopSeparator;
            if (headerViewCell == null) return;

            var firstVisibleIndexPath = TableView.IndexPathsForVisibleRows.First();
            if (firstVisibleIndexPath.Section == section)
            {
                var nextHeader = TableView.GetHeaderView(section + 1) as IHeaderViewCellWithHideableTopSeparator;
                if (nextHeader == null) return;
                nextHeader.TopSeparatorHidden = false;
                headerViewCell.TopSeparatorHidden = true;
            }
            else
            {
                headerViewCell.TopSeparatorHidden = false;
            }
        }

        protected virtual UITableViewHeaderFooterView GetOrCreateHeaderViewFor(UITableView tableView)
            => tableView.DequeueReusableHeaderFooterView(headerCellIdentifier);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);

        public override IEnumerable ItemsSource
        {
            get => base.ItemsSource;
            set
            {
                if (AnimatableCollection != null)
                {
                    AnimatableCollection.OnChildCollectionChanged -= OnChildCollectionChanged;
                }

                base.ItemsSource = value;

                if (AnimatableCollection != null)
                {
                    AnimatableCollection.OnChildCollectionChanged += OnChildCollectionChanged;
                }
            }
        }

        protected override void CollectionChangedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            InvokeOnMainThread(() => 
            {
                if (!UseAnimations)
                {
                    ReloadTableData();
                    return;
                }

                try
                {
                    animateSectionChangesIfPossible(args);
                }
                catch
                {
                    ReloadTableData();
                }

            });
        }

        protected virtual void OnSectionRemoved(NSIndexSet indexToRemove)
        {
            TableView.DeleteSections(indexToRemove, UITableViewRowAnimation.Automatic);
        }

        protected void OnChildCollectionChanged(object sender, ChildCollectionChangedEventArgs args)
        {
            InvokeOnMainThread(() =>
            {
                if (!UseAnimations)
                {
                    ReloadTableData();
                    return;
                }

                try
                {
                    animateRowChangesIfPossible(args);
                }
                catch
                {
                    ReloadTableData();
                }
            });
        }

        private void animateSectionChangesIfPossible(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnSectionAdded(NSIndexSet.FromIndex(args.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var indexToRemove = NSIndexSet.FromIndex(args.OldStartingIndex);
                    TableView.DeleteSections(indexToRemove, UITableViewRowAnimation.Automatic);
                    break;
            }
        }

        protected virtual void OnSectionAdded(NSIndexSet indexToAdd)
        {
            TableView.InsertSections(indexToAdd, UITableViewRowAnimation.Automatic);
        }

        private void animateRowChangesIfPossible(ChildCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var indexPathsToAdd = args.Indexes
                        .Select(row => NSIndexPath.FromRowSection(row, args.CollectionIndex))
                        .ToArray();
                    TableView.InsertRows(indexPathsToAdd, AddAnimation);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var indexPathsToRemove = args.Indexes
                        .Select(row => NSIndexPath.FromRowSection(row, args.CollectionIndex))
                        .ToArray();

                    TableView.DeleteRows(indexPathsToRemove, RemoveAnimation);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var indexPathsToUpdate = args.Indexes
                        .Select(row => NSIndexPath.FromRowSection(row, args.CollectionIndex))
                        .ToArray();

                    TableView.ReloadRows(indexPathsToUpdate, ReplaceAnimation);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing || AnimatableCollection == null) return;

            AnimatableCollection.OnChildCollectionChanged -= OnChildCollectionChanged;
        }
    }
}

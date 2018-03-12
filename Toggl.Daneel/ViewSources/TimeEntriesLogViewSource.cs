using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Views;
using Toggl.Daneel.Views.Log;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class TimeEntriesLogViewSource
        : GroupedCollectionTableViewSource<TimeEntryViewModelCollection, TimeEntryViewModel>,
          IUITableViewDataSource
    {
        private const int bottomPadding = 64;
        private const int emptyStateCellCount = 3;
        private const int spaceBetweenSections = 20;

        private const string cellIdentifier = nameof(TimeEntriesLogViewCell);
        private const string headerCellIdentifier = nameof(TimeEntriesLogHeaderViewCell);
        private const string emptyStateCellIdentifier = nameof(TimeEntriesLogEmptyStateViewCell);

        //Using the old API so that delete action would work on pre iOS 11 devices
        private readonly UITableViewRowAction deleteTableViewRowAction;

        private bool shouldUseEmptyStateCells
            => IsEmptyState && ItemsSource.Count() == 0;

        public bool IsEmptyState { get; set; }

        public IMvxAsyncCommand<TimeEntryViewModel> ContinueTimeEntryCommand { get; set; }

        public IMvxAsyncCommand<TimeEntryViewModel> DeleteTimeEntryCommand { get; set; }

        public new IMvxCommand<TimeEntryViewModel> SelectionChangedCommand { get; set; }

        public TimeEntriesLogViewSource(UITableView tableView)
            : base(tableView, cellIdentifier, headerCellIdentifier)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(TimeEntriesLogViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForCellReuse(TimeEntriesLogEmptyStateViewCell.Nib, emptyStateCellIdentifier);
            tableView.RegisterNibForHeaderFooterViewReuse(TimeEntriesLogHeaderViewCell.Nib, headerCellIdentifier);

            deleteTableViewRowAction = UITableViewRowAction.Create(
                UITableViewRowActionStyle.Destructive,
                Resources.Delete,
                handleDeleteTableViewRowAction);
            deleteTableViewRowAction.BackgroundColor = Color.TimeEntriesLog.DeleteSwipeActionBackground.ToNativeColor();
        }

        public override UIView GetViewForFooter(UITableView tableView, nint section)
            => new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, spaceBetweenSections));

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(tableView, indexPath, item);

            if (cell is IMvxBindable bindable)
                bindable.DataContext = item;

            if (cell is TimeEntriesLogViewCell logCell)
                logCell.ContinueTimeEntryCommand = ContinueTimeEntryCommand;

            if (cell is TimeEntriesLogEmptyStateViewCell)
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        protected override IEnumerable<TimeEntryViewModel> GetGroupAt(nint section)
            => GroupedItems.ElementAtOrDefault((int)section);

        public override nfloat GetHeightForHeader(UITableView tableView, nint section) => 48;

        public override nfloat GetHeightForFooter(UITableView tableView, nint section) => spaceBetweenSections;

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 64;

        public override bool ShouldScrollToTop(UIScrollView scrollView) => true;

        public override nint NumberOfSections(UITableView tableView)
        {
            if (shouldUseEmptyStateCells)
                return 1;

            return base.NumberOfSections(tableView);
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (shouldUseEmptyStateCells)
                return emptyStateCellCount;

            return base.RowsInSection(tableview, section);
        }

        public new object GetItemAt(NSIndexPath indexPath)
        {
            if (shouldUseEmptyStateCells)
                return null;

            return base.GetItemAt(indexPath);
        }

        public new UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifierForItem(item), indexPath);

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            if (item == null)
                return new UITableViewRowAction[0];
            return new[] { deleteTableViewRowAction };
        }

        public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                return null;

            var item = GetItemAt(indexPath);
            if (item == null)
                return null;

            return UISwipeActionsConfiguration
                .FromActions(new[] { continueSwipeActionFor((TimeEntryViewModel)item) });
        }

        protected override void OnSectionAdded(NSIndexSet indexToAdd)
        {
            if (IsEmptyState)
                TableView.ReloadSections(indexToAdd, UITableViewRowAnimation.Automatic);
            else
                TableView.InsertSections(indexToAdd, UITableViewRowAnimation.Automatic);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);

            if (item is TimeEntryViewModel timeEntry)
                SelectionChangedCommand?.Execute(timeEntry);

            tableView.DeselectRow(indexPath, true);
        }

        public override bool ShouldHighlightRow(UITableView tableView, NSIndexPath rowIndexPath)
        {
            if (shouldUseEmptyStateCells)
                return false;

            return true;
        }

        private void handleDeleteTableViewRowAction(UITableViewRowAction _, NSIndexPath indexPath)
        {
            var timeEntry = (TimeEntryViewModel)GetItemAt(indexPath);
            DeleteTimeEntryCommand.Execute(timeEntry);
        }

        private UIContextualAction continueSwipeActionFor(TimeEntryViewModel timeEntry)
        {
            var continueAction = UIContextualAction.FromContextualActionStyle(
                UIContextualActionStyle.Normal,
                Resources.Continue,
                (action, sourceView, completionHandler) =>
                {
                    ContinueTimeEntryCommand.Execute(timeEntry);
                    completionHandler.Invoke(finished: true);
                }
            );
            continueAction.BackgroundColor = Color.TimeEntriesLog.ContinueSwipeActionBackground.ToNativeColor();
            return continueAction;
        }

        private string cellIdentifierForItem(object item)
        {
            if (item == null)
                return emptyStateCellIdentifier;

            if (item is TimeEntryViewModel)
                return cellIdentifier;

            throw new ArgumentException($"Unexpected item type. Must be either null or of type {nameof(TimeEntryViewModel)}");
        }
    }
}

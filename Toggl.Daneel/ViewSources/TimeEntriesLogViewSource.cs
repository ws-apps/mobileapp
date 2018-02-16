using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Views;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class TimeEntriesLogViewSource : GroupedCollectionTableViewSource<TimeEntryViewModelCollection, TimeEntryViewModel>
    {
        private const int bottomPadding = 64;

        private const string cellIdentifier = nameof(TimeEntriesLogViewCell);
        private const string headerCellIdentifier = nameof(TimeEntriesLogHeaderViewCell);

        //Using the old API so that delete action would work on pre iOS 11 devices
        private readonly UITableViewRowAction deleteTableViewRowAction;

        public IMvxAsyncCommand<TimeEntryViewModel> ContinueTimeEntryCommand { get; set; }

        public IMvxAsyncCommand<TimeEntryViewModel> DeleteTimeEntryCommand { get; set; }

        public TimeEntriesLogViewSource(UITableView tableView)
            : base(tableView, cellIdentifier, headerCellIdentifier)
        {
            tableView.TableFooterView = new UIView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, bottomPadding));
            tableView.TableFooterView.BackgroundColor = Color.TimeEntriesLog.SectionFooter.ToNativeColor();
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(TimeEntriesLogViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForHeaderFooterViewReuse(TimeEntriesLogHeaderViewCell.Nib, headerCellIdentifier);

            deleteTableViewRowAction = UITableViewRowAction.Create(
                UITableViewRowActionStyle.Destructive,
                Resources.Delete,
                handleDeleteTableViewRowAction);
            deleteTableViewRowAction.BackgroundColor = Color.TimeEntriesLog.DeleteSwipeActionBackground.ToNativeColor();
        }

        public override UIView GetViewForFooter(UITableView tableView, nint section)
        {
            var rect = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 24);
            var footerView = new UIView(rect)
            {
                BackgroundColor = Color.TimeEntriesLog.SectionFooter.ToNativeColor()
            };

            return footerView;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = base.GetCell(tableView, indexPath);

            if (cell is TimeEntriesLogViewCell logCell)
                logCell.ContinueTimeEntryCommand = ContinueTimeEntryCommand;

            return cell;
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section) => 43;

        public override nfloat GetHeightForFooter(UITableView tableView, nint section) => 24;

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 64;

        public override bool ShouldScrollToTop(UIScrollView scrollView)
        {
            scrollView.SetContentOffset(CGPoint.Empty, false);
            return false;
        }

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            => new[] { deleteTableViewRowAction };

        public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                return null;
            
            return UISwipeActionsConfiguration
                .FromActions(new[] { continueSwipeActionFor(indexPath) });
        }

        private void handleDeleteTableViewRowAction(UITableViewRowAction _, NSIndexPath indexPath)
        {
            var timeEntry = (TimeEntryViewModel)GetItemAt(indexPath);
            DeleteTimeEntryCommand.Execute(timeEntry);
        }

        private UIContextualAction continueSwipeActionFor(NSIndexPath indexPath)
        {
            var continueAction = UIContextualAction.FromContextualActionStyle(
                UIContextualActionStyle.Normal,
                Resources.Continue,
                (action, sourceView, completionHandler) =>
                {
                    var timeEntry = (TimeEntryViewModel)GetItemAt(indexPath);
                    ContinueTimeEntryCommand.Execute(timeEntry);
                    completionHandler.Invoke(finished: true);
                }
            );
            continueAction.BackgroundColor = Color.TimeEntriesLog.ContinueSwipeActionBackground.ToNativeColor();
            return continueAction;
        }
    }
}

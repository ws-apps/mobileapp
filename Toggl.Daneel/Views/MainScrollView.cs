using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.Sync;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(MainScrollView))]
    public sealed class MainScrollView : UIScrollView
    {
        internal const float SyncStateViewHeight = 26;
        private static readonly float scrollThreshold = 3 * SyncStateViewHeight;

        private readonly UIColor pullToRefreshColor = Color.Main.PullToRefresh.ToNativeColor();
        private readonly UIColor syncingColor = Color.Main.Syncing.ToNativeColor();
        private readonly UIColor syncFailedColor = Color.Main.SyncFailed.ToNativeColor();
        private readonly UIColor offlineColor = Color.Main.Offline.ToNativeColor();
        private readonly UIColor syncCompletedColor = Color.Main.SyncCompleted.ToNativeColor();

        private SyncProgress syncProgress;
        private bool wasReleased;
        private bool needsRefresh;
        private bool shouldCalculateOnDeceleration;
        private bool isSyncing => syncProgress == SyncProgress.Syncing;
        private int syncIndicatorLastShown;

        public SyncProgress SyncProgress
        {
            get => syncProgress;
            set
            {
                if (value == syncProgress) return;
                syncProgress = value;
                updateViews(value);
            }
        }

        public UIView SyncStateView { get; set; }

        public UILabel SyncStateLabel { get; set; }

        public UIImageView DismissSyncBarImageView { get; set; }

        public IMvxCommand RefreshCommand { get; set; }

        public MainScrollView(IntPtr handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Scrolled += onScrolled;
            DraggingStarted += onDragStarted;
            DraggingEnded += onDragEnded;
            DecelerationEnded += onDecelerationEnded;
            ShouldScrollToTop = shouldScrollToTop;

            Bounces = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            Scrolled -= onScrolled;
            DraggingStarted -= onDragStarted;
            DraggingEnded -= onDragEnded;
            DecelerationEnded -= onDecelerationEnded;
        }

        private void onScrolled(object sender, EventArgs e)
        {
            if (!Dragging || wasReleased) return;

            var offset = ContentOffset.Y;
            if (offset >= 0) return;

            var needsMorePulling = Math.Abs(offset) < scrollThreshold;
            needsRefresh = !needsMorePulling;

            if (isSyncing) return;

            DismissSyncBarImageView.Hidden = true;
            setSyncIndicatorTextAndBackground(
                new NSAttributedString(needsMorePulling ? Resources.PullDownToRefresh : Resources.ReleaseToRefresh),
                pullToRefreshColor);
        }

        private void onDragStarted(object sender, EventArgs e)
        {
            wasReleased = false;
        }

        private void onDragEnded(object sender, DraggingEventArgs e)
        {
            var offset = ContentOffset.Y;
            if (offset >= 0) return;

            shouldCalculateOnDeceleration = e.Decelerate;
            wasReleased = true;

            if (shouldCalculateOnDeceleration) return;
            refreshIfNeeded();
        }

        private void onDecelerationEnded(object sender, EventArgs e)
        {
            if (!shouldCalculateOnDeceleration) return;
            refreshIfNeeded();
        }

        private void refreshIfNeeded()
        {
            if (!needsRefresh)
            {
                scrollToIfInFirstPage(CGPoint.Empty);
                return;
            }

            needsRefresh = false;
            shouldCalculateOnDeceleration = false;

            if (isSyncing)
            {
                scrollToIfInFirstPage(new CGPoint(0, -SyncStateViewHeight));
                return;
            }

            RefreshCommand.Execute();
        }

        private async void updateViews(SyncProgress value)
        {
            bool hideIndicator = false;

            switch (value)
            {
                case SyncProgress.Unknown:
                    return;

                case SyncProgress.Syncing:
                    setSyncIndicatorTextAndBackground(
                        new NSAttributedString(Resources.Syncing),
                        syncingColor);
                    break;

                case SyncProgress.OfflineModeDetected:
                    setSyncIndicatorTextAndBackground(
                        Resources.Offline.EndingWithRefreshIcon(SyncStateLabel.Font.CapHeight),
                        offlineColor);
                    DismissSyncBarImageView.Hidden = false;
                    break;

                case SyncProgress.Synced:
                    setSyncIndicatorTextAndBackground(
                        Resources.SyncCompleted.EndingWithTick(SyncStateLabel.Font.CapHeight),
                        syncCompletedColor);
                    hideIndicator = true;
                    break;

                case SyncProgress.Failed:
                    setSyncIndicatorTextAndBackground(
                        Resources.SyncFailed.EndingWithRefreshIcon(SyncStateLabel.Font.CapHeight),
                        syncFailedColor);
                    DismissSyncBarImageView.Hidden = false;
                    break;

                default:
                    throw new ArgumentException(nameof(value));
            }

            int syncIndicatorShown = showSyncIndicator();

            if (!hideIndicator) return;

            await hideSyncIndicator(syncIndicatorShown);
        }
        
        private void setSyncIndicatorTextAndBackground(NSAttributedString text, UIColor backgroundColor)
        {
            Animate(Animation.Timings.EnterTiming, () =>
            {
                SyncStateLabel.AttributedText = text;
                SyncStateView.BackgroundColor = backgroundColor;
            });
        }

        private int showSyncIndicator()
        {
            scrollToIfInFirstPage(new CGPoint(0, -SyncStateViewHeight));
            return ++syncIndicatorLastShown;
        }

        private async Task hideSyncIndicator(int syncIndicatorShown)
        {
            await Task.Delay(Animation.Timings.HideSyncStateViewDelay);

            if (syncIndicatorShown != syncIndicatorLastShown) return;

            scrollToIfInFirstPage(CGPoint.Empty);
        }

        private void scrollToIfInFirstPage(CGPoint offset)
        {
            if (ContentOffset.Y > 0) return;

            SetContentOffset(offset, true);
        }

        private bool shouldScrollToTop(UIScrollView scrollView)
        {
            scrollView.SetContentOffset(CGPoint.Empty, true);
            return false;
        }
    }
}

﻿using CoreGraphics;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;
using UIKit;
using static Toggl.Daneel.Extensions.LayoutConstraintExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public sealed partial class ReportsViewController : MvxViewController<ReportsViewModel>
    {
        private const int calendarHeight = 338;

        private UIButton titleButton;

        private ReportsTableViewSource source;

        internal UIView CalendarContainerView => CalendarContainer;

        internal bool CalendarIsVisible { get; private set; }

        public ReportsViewController() : base(nameof(ReportsViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            source = new ReportsTableViewSource(ReportsTableView);
            source.OnScroll += onReportsTableScrolled;
            ReportsTableView.Source = source;

            var bindingSet = this.CreateBindingSet<ReportsViewController, ReportsViewModel>();

            bindingSet.Bind(source).To(vm => vm.Segments);
            bindingSet.Bind(titleButton).To(vm => vm.ToggleCalendarCommand);
            bindingSet.Bind(titleButton)
                      .For(v => v.BindTitle())
                      .To(vm => vm.CurrentDateRangeString);

            bindingSet.Bind(source)
                      .For(v => v.ViewModel)
                      .To(vm => vm);

            bindingSet.Bind(ReportsTableView)
                      .For(v => v.BindTap())
                      .To(vm => vm.HideCalendarCommand);

            bindingSet.Apply();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            source.OnScroll -= onReportsTableScrolled;
        }

        private void onReportsTableScrolled(object sender, CGPoint offset)
        {
            if (CalendarIsVisible)
            {
                var topConstant = (TopCalendarConstraint.Constant + offset.Y).Clamp(0, calendarHeight);
                TopCalendarConstraint.Constant = topConstant;

                if (topConstant == 0) return;

                // we need to adjust the offset of the scroll view so that it doesn't fold
                // under the calendar while scrolling up
                var adjustedOffset = new CGPoint(offset.X, ReportsTableView.ContentOffset.Y - offset.Y);
                ReportsTableView.SetContentOffset(adjustedOffset, false);
                View.LayoutIfNeeded();

                if (topConstant == calendarHeight)
                {
                    HideCalendar();
                }
            }
        }

        internal void ShowCalendar()
        {
            TopCalendarConstraint.Constant = 0;
            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.SharpCurve,
                () => View.LayoutSubviews(),
                () => CalendarIsVisible = true);
        }

        internal void HideCalendar()
        {
            TopCalendarConstraint.Constant = calendarHeight;
            AnimationExtensions.Animate(
                Animation.Timings.EnterTiming,
                Animation.Curves.SharpCurve,
                () => View.LayoutSubviews(),
                () => CalendarIsVisible = false);
        }

        private void prepareViews()
        {
            TopConstraint.AdaptForIos10(NavigationController.NavigationBar);

            // Title view
            NavigationItem.TitleView = titleButton = new UIButton(new CGRect(0, 0, 200, 40));
            titleButton.SetTitleColor(UIColor.Black, UIControlState.Normal);

            // Calendar configuration
            TopCalendarConstraint.Constant = calendarHeight;
        }
    }
}


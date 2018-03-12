using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Converters;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Combiners;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using static Toggl.Foundation.MvvmCross.Helper.Animation;

namespace Toggl.Daneel.Views.Reports
{
    public partial class ReportsHeaderView : MvxTableViewHeaderFooterView
    {
        public static readonly NSString Key = new NSString(nameof(ReportsHeaderView));
        public static readonly UINib Nib;

        static ReportsHeaderView()
        {
            Nib = UINib.FromName(nameof(ReportsHeaderView), NSBundle.MainBundle);
        }

        public ReportsHeaderView(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var templateImage = TotalDurationGraph.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            TotalDurationGraph.Image = templateImage;

            var durationCombiner = new DurationValueCombiner();

            this.DelayBind(() =>
            {
                var bindingSet = this.CreateBindingSet<ReportsHeaderView, ReportsViewModel>();

                //Text
                bindingSet.Bind(BillablePercentageLabel)
                          .For(v => v.AttributedText)
                          .To(vm => vm.BillablePercentage)
                          .WithConversion(new ReportPercentageLabelValueConverter());

                bindingSet.Bind(TotalDurationLabel)
                          .For(v => v.Text)
                          .ByCombining(durationCombiner,
                              vm => vm.TotalTime,
                              vm => vm.DurationFormat);

                //Loading chart
                bindingSet.Bind(LoadingPieChartView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.IsLoading);

                //Pretty stuff
                bindingSet.Bind(PieChartView)
                          .For(v => v.Segments)
                          .To(vm => vm.Segments);

                bindingSet.Bind(BillablePercentageView)
                          .For(v => v.Percentage)
                          .To(vm => vm.BillablePercentage);

                bindingSet.Bind(TotalDurationGraph)
                          .For(v => v.TintColor)
                          .To(vm => vm.TotalTimeIsZero)
                          .WithConversion(new BoolToConstantValueConverter<UIColor>(
                              Color.Reports.Disabled.ToNativeColor(),
                              Color.Reports.TotalTimeActivated.ToNativeColor()
                          ));

                //Visibility
                bindingSet.Bind(EmptyStateView)
                          .For(v => v.BindVisible())
                          .To(vm => vm.ShowEmptyState);

                bindingSet.Apply();
            });
        }
    }
}

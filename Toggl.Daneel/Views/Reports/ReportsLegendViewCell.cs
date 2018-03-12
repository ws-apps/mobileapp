using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Plugins.Color;
using Toggl.Foundation.MvvmCross.Combiners;
using Toggl.Foundation.Reports;
using UIKit;

namespace Toggl.Daneel.Views.Reports
{
    public partial class ReportsLegendViewCell : MvxTableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ReportsLegendViewCell));
        public static readonly UINib Nib;

        static ReportsLegendViewCell()
        {
            Nib = UINib.FromName(nameof(ReportsLegendViewCell), NSBundle.MainBundle);
        }

        protected ReportsLegendViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.DelayBind(() =>
            {
                var colorConverter = new MvxRGBValueConverter();
                var durationCombiner = new DurationValueCombiner();
                var bindingSet = this.CreateBindingSet<ReportsLegendViewCell, ChartSegment>();

                //Text
                bindingSet.Bind(ProjectLabel).To(vm => vm.ProjectName);
                bindingSet.Bind(PercentageLabel)
                          .For(v => v.Text)
                          .ByCombining("Format", "'{0:0.00}%'", nameof(ChartSegment.Percentage));

                bindingSet.Bind(TotalTimeLabel)
                          .ByCombining(durationCombiner,
                              vm => vm.TrackedTime,
                              vm => vm.DurationFormat);

                // Color
                bindingSet.Bind(ProjectLabel)
                          .For(v => v.TextColor)
                          .To(vm => vm.Color)
                          .WithConversion(colorConverter);

                bindingSet.Bind(CircleView)
                          .For(v => v.BackgroundColor)
                          .To(vm => vm.Color)
                          .WithConversion(colorConverter);
                
                bindingSet.Apply();
            });
        }
    }
}

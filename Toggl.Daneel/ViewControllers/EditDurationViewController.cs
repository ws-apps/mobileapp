using CoreGraphics;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using static Toggl.Daneel.Extensions.FontExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class EditDurationViewController : MvxViewController<EditDurationViewModel>
    {
        public EditDurationViewController() : base(nameof(EditDurationViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var timeConverter = new DateTimeToTimeValueConverter();
            var dateConverter = new DateToTitleStringValueConverter();
            var durationConverter = new TimeSpanToDurationValueConverter();
            var inverseBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var editedTimeLabelColorConverter = new BoolToConstantValueConverter<UIColor>(
                Color.EditDuration.EditedTime.ToNativeColor(),
                Color.EditDuration.NotEditedTime.ToNativeColor());

            var bindingSet = this.CreateBindingSet<EditDurationViewController, EditDurationViewModel>();

            //Commands
            bindingSet.Bind(SaveButton).To(vm => vm.SaveCommand);
            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);

            //Start and stop date/time
            bindingSet.Bind(StartTimeLabel)
                      .To(vm => vm.StartTime)
                      .WithConversion(timeConverter);

            bindingSet.Bind(StartDateLabel)
                      .To(vm => vm.StartTime)
                      .WithConversion(dateConverter);

            bindingSet.Bind(EndTimeLabel)
                      .To(vm => vm.StopTime)
                      .WithConversion(timeConverter);

            bindingSet.Bind(EndDateLabel)
                      .To(vm => vm.StopTime)
                      .WithConversion(dateConverter);

            //Visiblity
            bindingSet.Bind(EndTimeLabel)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsRunning);

            bindingSet.Bind(EndDateLabel)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsRunning);

            //Stop time entry button
            bindingSet.Bind(SetEndButton)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsRunning)
                      .WithConversion(inverseBoolConverter);

            bindingSet.Bind(SetEndButton)
                      .To(vm => vm.StopTimeEntryCommand);

            //The wheel
            bindingSet.Bind(DurationLabel)
                      .To(vm => vm.Duration)
                      .WithConversion(durationConverter);

            bindingSet.Bind(WheelView)
                      .For(v => v.MaximumStartTime)
                      .To(vm => vm.MaximumStartTime);

            bindingSet.Bind(WheelView)
                      .For(v => v.MinimumStartTime)
                      .To(vm => vm.MinimumStartTime);

            bindingSet.Bind(WheelView)
                      .For(v => v.MaximumEndTime)
                      .To(vm => vm.MaximumStopTime);

            bindingSet.Bind(WheelView)
                      .For(v => v.MinimumEndTime)
                      .To(vm => vm.MinimumStopTime);

            bindingSet.Bind(WheelView)
                      .For(v => v.StartTime)
                      .To(vm => vm.StartTime);

            bindingSet.Bind(WheelView)
                      .For(v => v.EndTime)
                      .To(vm => vm.StopTime);

            bindingSet.Bind(WheelView)
                      .For(v => v.IsRunning)
                      .To(vm => vm.IsRunning);

            bindingSet.Apply();
        }

        private void prepareViews()
        {

            EndTimeLabel.Font = EndTimeLabel.Font.GetMonospacedDigitFont();
            DurationLabel.Font = DurationLabel.Font.GetMonospacedDigitFont();
            StartTimeLabel.Font = StartTimeLabel.Font.GetMonospacedDigitFont();

            SetEndButton.TintColor = Color.EditDuration.SetButton.ToNativeColor();
        }
    }
}


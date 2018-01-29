using CoreGraphics;
using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using static Toggl.Daneel.Extensions.FontExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalDialogPresentation]
    public sealed partial class EditDurationViewController : MvxViewController<EditDurationViewModel>
    {
        private const int maxDurationViewHeight = 424;
        private const int minDurationViewHeight = 72;

        public EditDurationViewController() : base(nameof(EditDurationViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var timeConverter = new DateTimeToTimeValueConverter();
            var durationConverter = new TimeSpanToDurationValueConverter();
            var timeSpanToHeightConverter = new TimeSpanToViewHeightConverter(minDurationViewHeight, maxDurationViewHeight);

            var bindingSet = this.CreateBindingSet<EditDurationViewController, EditDurationViewModel>();

            //Commands
            bindingSet.Bind(SaveButton).To(vm => vm.SaveCommand);
            bindingSet.Bind(CloseButton).To(vm => vm.CloseCommand);

            //Text
            bindingSet.Bind(StartTimeLabel)
                      .To(vm => vm.StartTime)
                      .WithConversion(timeConverter);

            bindingSet.Bind(EndTimeLabel)
                      .To(vm => vm.StopTime)
                      .WithConversion(timeConverter);

            bindingSet.Bind(DurationLabel)
                      .To(vm => vm.Duration)
                      .WithConversion(durationConverter);

            //Size
            bindingSet.Bind(DurationView)
                      .For(v => v.HeightConstant)
                      .To(vm => vm.Duration)
                      .WithConversion(timeSpanToHeightConverter);
            
            bindingSet.Apply();
        }

        private void prepareViews()
        {
            PreferredContentSize = new CGSize
            {
                //32 for 16pt margins on both sides
                Width = UIScreen.MainScreen.Bounds.Width - 32,
                Height = 480
            };

            EndTimeLabel.Font = EndTimeLabel.Font.GetMonospacedDigitFont();
            DurationLabel.Font = DurationLabel.Font.GetMonospacedDigitFont();
            StartTimeLabel.Font = StartTimeLabel.Font.GetMonospacedDigitFont();

            DurationView.MinHeight = minDurationViewHeight;
            DurationView.MaxHeight = maxDurationViewHeight;
        }
    }
}


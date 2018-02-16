using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.Presentation.Transition;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using static Toggl.Daneel.Extensions.FontExtensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class EditDurationViewController : KeyboardAwareViewController<EditDurationViewModel>
    {
        private const int offsetFromSafeAreaTop = 20;
        private const int bottomOffset = 48;

        public EditDurationViewController() : base(nameof(EditDurationViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            setupDismissingByTappingOnBackground();
            prepareViews();

            var timeConverter = new DateTimeToTimeValueConverter();
            var dateConverter = new DateToTitleStringValueConverter();
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
            bindingSet.Bind(DurationInput)
                      .For(v => v.Duration)
                      .To(vm => vm.Duration);

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

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if (DurationInput.IsEditing)
                DurationInput.EndEditing(true);
        }

        private void setupDismissingByTappingOnBackground()
        {
            if (PresentationController is ModalPresentationController modalPresentationController)
            {
                var tapToDismiss = new UITapGestureRecognizer(() => ViewModel.CloseCommand.Execute());
                modalPresentationController.AdditionalContentView.AddGestureRecognizer(tapToDismiss);
            }
        }

        public override void ViewWillLayoutSubviews()
        {
            var height = WheelView.Frame.Bottom + bottomOffset;
            var newSize = new CGSize(0, height);
            if (newSize != PreferredContentSize)
            {
                PreferredContentSize = newSize;
                PresentationController.ContainerViewWillLayoutSubviews();
            }
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            nfloat distanceFromTop = offsetFromSafeAreaTop;
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                distanceFromTop += UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Top;
            }

            View.Frame = new CGRect(0, distanceFromTop, View.Frame.Width, View.Frame.Height);
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            var height = WheelView.Frame.Bottom + bottomOffset;
            var offsetFromTop = UIScreen.MainScreen.Bounds.Height - height;
            View.Frame = new CGRect(0, offsetFromTop, View.Frame.Width, View.Frame.Height);
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        private void prepareViews()
        {
            DurationInput.Converter = new TimeSpanToDurationValueConverter();

            EndTimeLabel.Font = EndTimeLabel.Font.GetMonospacedDigitFont();
            StartTimeLabel.Font = StartTimeLabel.Font.GetMonospacedDigitFont();

            SetEndButton.TintColor = Color.EditDuration.SetButton.ToNativeColor();
        }
    }
}


﻿using System;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.Presentation.Transition;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Combiners;
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
        private const int stackViewSpacing = 26;
        private bool viewDidAppear = false;

        public EditDurationViewController() : base(nameof(EditDurationViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            setupDismissingByTappingOnBackground();
            prepareViews();

            var durationCombiner = new DurationValueCombiner();
            var timeCombiner = new DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo.Local);
            var dateCombiner = new DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo.Local, useLongFormat: false);
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
                      .ByCombining(timeCombiner,
                          vm => vm.StartTime,
                          vm => vm.TimeFormat);

            bindingSet.Bind(StartDateLabel)
                      .ByCombining(dateCombiner,
                          vm => vm.StartTime,
                          vm => vm.DateFormat);

            bindingSet.Bind(EndTimeLabel)
                      .ByCombining(timeCombiner,
                          vm => vm.StopTime,
                          vm => vm.TimeFormat);

            bindingSet.Bind(EndDateLabel)
                      .ByCombining(dateCombiner,
                          vm => vm.StopTime,
                          vm => vm.DateFormat);

            //Editing start and end time
            bindingSet.Bind(StartView)
                      .For(v => v.BindTap())
                      .To(vm => vm.EditStartTimeCommand);

            bindingSet.Bind(EndView)
                      .For(v => v.BindTap())
                      .To(vm => vm.EditStopTimeCommand);

            bindingSet.Bind(SetEndButton)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsRunning)
                      .WithConversion(inverseBoolConverter);

            bindingSet.Bind(SetEndButton)
                      .To(vm => vm.EditStopTimeCommand);

            //Visiblity
            bindingSet.Bind(EndTimeLabel)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsRunning);

            bindingSet.Bind(EndDateLabel)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.IsRunning);

            //Stard and end colors
            bindingSet.Bind(StartTimeLabel)
                      .For(v => v.TextColor)
                      .To(vm => vm.IsEditingStartTime)
                      .WithConversion(editedTimeLabelColorConverter);

            bindingSet.Bind(StartDateLabel)
                      .For(v => v.TextColor)
                      .To(vm => vm.IsEditingStartTime)
                      .WithConversion(editedTimeLabelColorConverter);

            bindingSet.Bind(EndTimeLabel)
                      .For(v => v.TextColor)
                      .To(vm => vm.IsEditingStopTime)
                      .WithConversion(editedTimeLabelColorConverter);

            bindingSet.Bind(EndDateLabel)
                      .For(v => v.TextColor)
                      .To(vm => vm.IsEditingStopTime)
                      .WithConversion(editedTimeLabelColorConverter);

            //Date picker
            bindingSet.Bind(DatePickerContainer)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.IsEditingTime);

            bindingSet.Bind(DatePicker)
                      .For(v => v.BindDateTimeOffset())
                      .To(vm => vm.EditedTime);

            bindingSet.Bind(DatePicker)
                      .For(v => v.MaximumDate)
                      .To(vm => vm.MaximumDateTime);

            bindingSet.Bind(DatePicker)
                      .For(v => v.MinimumDate)
                      .To(vm => vm.MinimumDateTime);

            //The wheel
            bindingSet.Bind(DurationInput)
                      .For(v => v.UserInteractionEnabled)
                      .To(vm => vm.IsEditingTime)
                      .WithConversion(inverseBoolConverter);

            bindingSet.Bind(DurationInput)
                      .For(v => v.Duration)
                      .To(vm => vm.Duration);

            bindingSet.Bind(DurationInput)
                      .For(v => v.FormattedDuration)
                      .ByCombining(durationCombiner,
                          vm => vm.Duration,
                          vm => vm.DurationFormat);

            bindingSet.Bind(WheelView)
                      .For(v => v.UserInteractionEnabled)
                      .To(vm => vm.IsEditingTime)
                      .WithConversion(inverseBoolConverter);

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

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            adjustHeight();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            viewDidAppear = true;
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
            var frame = View.ConvertRectFromView(WheelView.Frame, WheelView.Superview);
            var height = frame.Bottom + bottomOffset;

            if (ViewModel.IsEditingTime)
            {
                height -= DatePickerContainer.Frame.Height + stackViewSpacing;
            }

            var offsetFromTop = UIScreen.MainScreen.Bounds.Height - height;
            View.Frame = new CGRect(0, offsetFromTop, View.Frame.Width, View.Frame.Height);
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        private void adjustHeight()
        {
            if (viewDidAppear) return;

            var frame = View.ConvertRectFromView(WheelView.Frame, WheelView.Superview);
            var height = frame.Bottom + bottomOffset;
            var newSize = new CGSize(0, height);
            if (newSize != PreferredContentSize)
            {
                PreferredContentSize = newSize;
                PresentationController.ContainerViewWillLayoutSubviews();
            }
        }

        private void setupDismissingByTappingOnBackground()
        {
            if (PresentationController is ModalPresentationController modalPresentationController)
            {
                var tapToDismiss = new UITapGestureRecognizer(() => ViewModel.CloseCommand.Execute());
                modalPresentationController.AdditionalContentView.AddGestureRecognizer(tapToDismiss);
            }
        }

        private void prepareViews()
        {
            EndTimeLabel.Font = EndTimeLabel.Font.GetMonospacedDigitFont();
            StartTimeLabel.Font = StartTimeLabel.Font.GetMonospacedDigitFont();

            SetEndButton.TintColor = Color.EditDuration.SetButton.ToNativeColor();

            StackView.Spacing = stackViewSpacing;

            var backgroundTap = new UITapGestureRecognizer(onBackgroundTap);
            View.AddGestureRecognizer(backgroundTap);

            var editTimeTap = new UITapGestureRecognizer(onEditTimeTap);
            StartTimeLabel.AddGestureRecognizer(editTimeTap);
            EndTimeLabel.AddGestureRecognizer(editTimeTap);
        }

        private void onEditTimeTap(UITapGestureRecognizer recognizer)
        {
            if (DurationInput.IsEditing)
                DurationInput.ResignFirstResponder();
        }

        private void onBackgroundTap(UITapGestureRecognizer recognizer)
        {
            if (DurationInput.IsEditing)
                DurationInput.ResignFirstResponder();

            if (ViewModel.IsEditingTime)
                ViewModel.StopEditingTimeCommand.Execute();
        }
    }
}


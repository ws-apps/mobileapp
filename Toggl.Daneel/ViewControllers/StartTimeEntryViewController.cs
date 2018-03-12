﻿using System;
using System.Collections.Generic;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Binding.iOS;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Plugins.Visibility;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using Toggl.Foundation;
using Foundation;
using Toggl.Multivac;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public sealed partial class StartTimeEntryViewController : KeyboardAwareViewController<StartTimeEntryViewModel>
    {
        public StartTimeEntryViewController() 
            : base(nameof(StartTimeEntryViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var source = new StartTimeEntryTableViewSource(SuggestionsTableView);
            SuggestionsTableView.Source = source;
            source.ToggleTasksCommand = new MvxCommand<ProjectSuggestion>(toggleTaskSuggestions);

            var parametricDurationConverter = new ParametricTimeSpanToDurationValueConverter();
            var invertedVisibilityConverter = new MvxInvertedVisibilityValueConverter();
            var invertedBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var buttonColorConverter = new BoolToConstantValueConverter<UIColor>(
                Color.StartTimeEntry.ActiveButton.ToNativeColor(),
                Color.StartTimeEntry.InactiveButton.ToNativeColor()
            );

            var bindingSet = this.CreateBindingSet<StartTimeEntryViewController, StartTimeEntryViewModel>();

            //TableView
            bindingSet.Bind(source).To(vm => vm.Suggestions);

            bindingSet.Bind(source)
                      .For(v => v.UseGrouping)
                      .To(vm => vm.UseGrouping);

            bindingSet.Bind(source)
                      .For(v => v.SelectSuggestionCommand)
                      .To(vm => vm.SelectSuggestionCommand);

            bindingSet.Bind(source)
                      .For(v => v.CreateCommand)
                      .To(vm => vm.CreateCommand);
            
            bindingSet.Bind(source)
                      .For(v => v.IsSuggestingProjects)
                      .To(vm => vm.IsSuggestingProjects);
            
            bindingSet.Bind(source)
                      .For(v => v.Text)
                      .To(vm => vm.CurrentQuery);

            bindingSet.Bind(source)
                      .For(v => v.SuggestCreation)
                      .To(vm => vm.SuggestCreation);

            bindingSet.Bind(source)
                      .For(v => v.ShouldShowNoTagsInfoMessage)
                      .To(vm => vm.ShouldShowNoTagsInfoMessage);

            bindingSet.Bind(source)
                      .For(v => v.ShouldShowNoProjectsInfoMessage)
                      .To(vm => vm.ShouldShowNoProjectsInfoMessage);

            //Text
            bindingSet.Bind(TimeInput)
                      .For(v => v.Duration)
                      .To(vm => vm.DisplayedTime);

            bindingSet.Bind(DescriptionTextView)
                      .For(v => v.BindTextFieldInfo())
                      .To(vm => vm.TextFieldInfo);

            bindingSet.Bind(TimeInput)
                      .For(v => v.FormattedDuration)
                      .To(vm => vm.DisplayedTime)
                      .WithConversion(parametricDurationConverter, DurationFormat.Improved);

            bindingSet.Bind(Placeholder)
                      .To(vm => vm.PlaceholderText);

            bindingSet.Bind(DescriptionRemainingLengthLabel)
                      .To(vm => vm.DescriptionRemainingBytes);

            //Buttons
            bindingSet.Bind(TagsButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.IsSuggestingTags)
                      .WithConversion(buttonColorConverter);
            
            bindingSet.Bind(BillableButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.IsBillable)
                      .WithConversion(buttonColorConverter);

            bindingSet.Bind(ProjectsButton)
                      .For(v => v.TintColor)
                      .To(vm => vm.IsSuggestingProjects)
                      .WithConversion(buttonColorConverter);

            bindingSet.Bind(DoneButton)
                      .For(v => v.Enabled)
                      .To(vm => vm.DescriptionLengthExceeded)
                      .WithConversion(invertedBoolConverter);

            //Visibility
            bindingSet.Bind(BillableButtonWidthConstraint)
                      .For(v => v.Constant)
                      .To(vm => vm.IsBillableAvailable)
                      .WithConversion(new BoolToConstantValueConverter<nfloat>(42, 0));

            bindingSet.Bind(DescriptionRemainingLengthLabel)
                      .For(v => v.BindVisible())
                      .To(vm => vm.DescriptionLengthExceeded)
                      .WithConversion(invertedVisibilityConverter);

            //Commands
            bindingSet.Bind(DoneButton).To(vm => vm.DoneCommand);
            bindingSet.Bind(CloseButton).To(vm => vm.BackCommand);
            bindingSet.Bind(BillableButton).To(vm => vm.ToggleBillableCommand);
            bindingSet.Bind(StartDateButton).To(vm => vm.SetStartDateCommand);
            bindingSet.Bind(DateTimeButton).To(vm => vm.ChangeTimeCommand);
            bindingSet.Bind(TagsButton).To(vm => vm.ToggleTagSuggestionsCommand);
            bindingSet.Bind(ProjectsButton).To(vm => vm.ToggleProjectSuggestionsCommand);

            bindingSet.Apply();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            BottomDistanceConstraint.Constant = e.FrameEnd.Height;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            BottomDistanceConstraint.Constant = 0;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            if (TimeInput.IsEditing)
            {
                TimeInput.EndEditing(true);
                DescriptionTextView.BecomeFirstResponder();
            }
        }

        private void prepareViews()
        {
            //This is needed for the ImageView.TintColor bindings to work
            foreach (var button in getButtons())
            {
                button.SetImage(
                    button.ImageForState(UIControlState.Normal)
                          .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    UIControlState.Normal
                );
                button.TintColor = Color.StartTimeEntry.InactiveButton.ToNativeColor();
            }

            TimeInput.TintColor = Color.StartTimeEntry.Cursor.ToNativeColor();

            DescriptionTextView.TintColor = Color.StartTimeEntry.Cursor.ToNativeColor();
            DescriptionTextView.BecomeFirstResponder();

            Placeholder.TextView = DescriptionTextView;
            Placeholder.Text = Resources.StartTimeEntryPlaceholder;
        }

        private IEnumerable<UIButton> getButtons()
        {
            yield return TagsButton;
            yield return ProjectsButton;
            yield return BillableButton;
            yield return StartDateButton;
            yield return DateTimeButton;
            yield return DateTimeButton;
        }

        private void toggleTaskSuggestions(ProjectSuggestion parameter)
        {
            var offset = SuggestionsTableView.ContentOffset;
            var frameHeight = SuggestionsTableView.Frame.Height;

            ViewModel.ToggleTaskSuggestionsCommand.Execute(parameter);

            SuggestionsTableView.CorrectOffset(offset, frameHeight);
        }
    }
}


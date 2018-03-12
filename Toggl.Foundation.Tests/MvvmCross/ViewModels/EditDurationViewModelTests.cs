﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class EditDurationViewModelTests
    {
        public abstract class EditDurationViewModelTest : BaseViewModelTests<EditDurationViewModel>
        {
            protected override EditDurationViewModel CreateViewModel()
                => new EditDurationViewModel(NavigationService, TimeService, DataSource);
        }

        public sealed class TheConstructor : EditDurationViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useNavigationService, bool useTimeService, bool useDataSource)
            {
                var navigationService = useNavigationService ? NavigationService : null;
                var timeService = useTimeService ? TimeService : null;
                var dataSource = useDataSource ? DataSource : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new EditDurationViewModel(navigationService, timeService, dataSource);

                tryingToConstructWithEmptyParameters.ShouldThrow<ArgumentNullException>();
            }

        }

        public sealed class TheDurationProperty : EditDurationViewModelTest
        {
            [Property]
            public void WhenChangedWhileUpdatingTheRunningTimeEntryTriggersTheUpdateOfTheStartTime(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(parameter);
                
                ViewModel.Duration = TimeSpan.FromHours(4);

                var expectedStart = start.AddHours(-2);
                ViewModel.StartTime.Should().BeSameDateAs(expectedStart);
            }

            [Property]
            public void WhenChangedWhileUpdatingFinishedTimeEntryTriggersTheUpdateOfTheStopTime(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, now - start);
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(parameter);
                
                ViewModel.Duration = TimeSpan.FromHours(4);

                var expectedStop = now.AddHours(2);
                ViewModel.StopTime.Should().BeSameDateAs(expectedStop);
            }

            [Property]
            public void IsUpdatedAccordingToTimeServiceForRunningTimeEntries(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                var tickSubject = new Subject<DateTimeOffset>();
                var tickObservable = tickSubject.AsObservable().Publish();
                tickObservable.Connect();
                TimeService.CurrentDateTimeObservable.Returns(tickObservable);
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(parameter);

                tickSubject.OnNext(now.AddHours(2));

                ViewModel.Duration.Hours.Should().Be(4);
            }
        }

        public sealed class TheDurationTimeProperty : EditDurationViewModelTest
        {
            [Property]
            public void IsUpdatedAccordingToTimeServiceForRunningTimeEntries(DateTimeOffset now, byte hours)
            {
                var duration = TimeSpan.FromHours(hours);
                var parameter = DurationParameter.WithStartAndDuration(now, null);
                var tickSubject = new Subject<DateTimeOffset>();
                var tickObservable = tickSubject.AsObservable().Publish();
                tickObservable.Connect();
                TimeService.CurrentDateTimeObservable.Returns(tickObservable);
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(parameter);

                var newCurrentTime = now + duration;
                tickSubject.OnNext(newCurrentTime);

                ViewModel.Duration.Should().Be(duration);
            }
        }

        public sealed class ThePrepareMethod : EditDurationViewModelTest
        {
            [Property]
            public void SetsTheStartTime(DateTimeOffset now)
            {
                var start = now;
                var parameter = DurationParameter.WithStartAndDuration(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.StartTime.Should().Be(start);
            }

            [Property]
            public void SetsTheStartTimeToCurrentTimeIfParameterDoesNotHaveStartTime(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                TimeService.CurrentDateTime.Returns(now);

                ViewModel.Prepare(parameter);

                ViewModel.StartTime.Should().BeSameDateAs(start);
            }

            [Property]
            public void SetsTheStopTimeToParameterStopTimeIfParameterHasStopTime(DateTimeOffset now)
            {
                var start = now.AddHours(-4);
                var stop = start.AddHours(2);
                var parameter = DurationParameter.WithStartAndDuration(start, stop - now);

                ViewModel.Prepare(parameter);

                ViewModel.StartTime.Should().BeSameDateAs(start);
            }

            [Property]
            public void SubscribesToCurrentTimeObservableIfParameterDoesNotHaveStopTime(DateTimeOffset now)
            {
                var parameter = DurationParameter.WithStartAndDuration(now, null);

                ViewModel.Prepare(parameter);

                TimeService.CurrentDateTimeObservable.Received().Subscribe(Arg.Any<AnonymousObserver<DateTimeOffset>>());
            }

            [Fact, LogIfTooSlow]
            public void SetsTheIsRunningPropertyWhenTheDurationIsNull()
            {
                var start = new DateTimeOffset(2018, 01, 15, 12, 34, 56, TimeSpan.Zero);
                var parameter = DurationParameter.WithStartAndDuration(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.IsRunning.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotSetTheIsRunningPropertyWhenTheDurationIsNotNull()
            {
                var start = new DateTimeOffset(2018, 01, 15, 12, 34, 56, TimeSpan.Zero);
                var duration = TimeSpan.FromMinutes(20);
                var parameter = DurationParameter.WithStartAndDuration(start, duration);

                ViewModel.Prepare(parameter);

                ViewModel.IsRunning.Should().BeFalse();
            }
        }

        public sealed class TheCloseCommand : EditDurationViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Any<DurationParameter>());
            }
            [Fact, LogIfTooSlow]
            public async Task ReturnsTheDefaultParameter()
            {
                var parameter = DurationParameter.WithStartAndDuration(DateTimeOffset.UtcNow, null);
                ViewModel.Prepare(parameter);

                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Is(parameter));
            }
        }

        public sealed class TheSaveCommand : EditDurationViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.SaveCommand.ExecuteAsync();

                await NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Any<DurationParameter>());
            }

            [Property]
            public void ReturnsAValueThatReflectsTheChangesToDurationForFinishedTimeEntries(DateTimeOffset start, DateTimeOffset stop)
            {
                if (start >= stop) return;

                var now = DateTimeOffset.UtcNow;
                TimeService.CurrentDateTime.Returns(now);
                if (start >= now) return;

                ViewModel.Prepare(DurationParameter.WithStartAndDuration(start, stop - start));
                ViewModel.Duration = TimeSpan.FromMinutes(10);

                ViewModel.SaveCommand.ExecuteAsync().Wait();

                NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Is<DurationParameter>(
                    p => p.Start == ViewModel.StartTime && p.Duration == ViewModel.Duration
                )).Wait();
            }

            [Property]
            public void ReturnsAValueThatReflectsTheChangesToDurationForRunningTimeEntries(DateTimeOffset start, DateTimeOffset now)
            {
                if (start > now) return;
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(DurationParameter.WithStartAndDuration(start, null));
                ViewModel.Duration = TimeSpan.FromMinutes(10);

                ViewModel.SaveCommand.ExecuteAsync().Wait();

                NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Is<DurationParameter>(
                    p => p.Start == ViewModel.StartTime && p.Duration == null
                )).Wait();
            }
        }

        public sealed class TheEditStartTimeCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenNothingWasEdited()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.IsEditingStartTime.Should().BeTrue();
                ViewModel.IsEditingStopTime.Should().BeFalse();
            }

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenStopTimeWasEdited()
            {
                ViewModel.Prepare(parameter);
                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.IsEditingStartTime.Should().BeTrue();
                ViewModel.IsEditingStopTime.Should().BeFalse();
            }

            [Fact]
            public void ClosesEditingWhenStartTimeWasBeingEdited()
            {
                ViewModel.Prepare(parameter);
                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.IsEditingStartTime.Should().BeFalse();
                ViewModel.IsEditingStopTime.Should().BeFalse();
            }

            [Fact]
            public void InitializesTheEditTimePropertyWithTheStartTime()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.EditedTime.Should().Be(parameter.Start);
            }

            [Fact]
            public void SetsTheMinimumAndMaximumDateForTheDatePicker()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.MinimumDateTime.Should().Be((parameter.Start + parameter.Duration.Value - TimeSpan.FromHours(999)).LocalDateTime);
                ViewModel.MaximumDateTime.Should().Be((parameter.Start + parameter.Duration.Value).LocalDateTime);
            }
        }

        public sealed class TheEditStopTimeCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenNothingWasEdited()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.IsEditingStartTime.Should().BeFalse();
                ViewModel.IsEditingStopTime.Should().BeTrue();
            }

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenStopTimeWasEdited()
            {
                ViewModel.Prepare(parameter);
                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.IsEditingStartTime.Should().BeFalse();
                ViewModel.IsEditingStopTime.Should().BeTrue();
            }

            [Fact]
            public void ClosesEditingWhenStartTimeWasBeingEdited()
            {
                ViewModel.Prepare(parameter);
                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.IsEditingStartTime.Should().BeFalse();
                ViewModel.IsEditingStopTime.Should().BeFalse();
            }

            [Fact]
            public void InitializesTheEditTimePropertyWithTheStartTime()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.EditedTime.Should().Be(parameter.Start + parameter.Duration.Value);
            }

            [Fact]
            public void SetsTheMinimumAndMaximumDateForTheDatePicker()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.MinimumDateTime.Should().Be(parameter.Start.LocalDateTime);
                ViewModel.MaximumDateTime.Should().Be((parameter.Start + TimeSpan.FromHours(999)).LocalDateTime);
            }

            [Fact]
            public void StopsARunningTimeEntry()
            {
                var now = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                var runningTEParameter = DurationParameter.WithStartAndDuration(parameter.Start, null);
                ViewModel.Prepare(runningTEParameter);
                TimeService.CurrentDateTime.Returns(now);

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.IsRunning.Should().BeFalse();
                ViewModel.StopTime.Should().Be(now);
            }

            [Fact]
            public void UnsubscribesFromTheTheRunningTimeEntryObservable()
            {
                var now = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                var runningTEParameter = DurationParameter.WithStartAndDuration(parameter.Start, null);
                var subject = new BehaviorSubject<DateTimeOffset>(now);
                var observable = subject.AsObservable().Publish();
                ViewModel.Prepare(runningTEParameter);
                TimeService.CurrentDateTime.Returns(now);
                TimeService.CurrentDateTimeObservable.Returns(observable);

                ViewModel.EditStopTimeCommand.Execute();
                subject.OnNext(now.AddSeconds(1));

                ViewModel.StopTime.Should().Be(now);
            }
        }

        public sealed class TheStopEditingTimeCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void ClearsAllTimeEditingFlagsWhenStartTimeWasEdited()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();
                ViewModel.StopEditingTimeCommand.Execute();

                ViewModel.IsEditingTime.Should().BeFalse();
                ViewModel.IsEditingStartTime.Should().BeFalse();
                ViewModel.IsEditingStopTime.Should().BeFalse();
            }

            [Fact]
            public void ClearsAllTimeEditingFlagsWhenStopTimeWasEdited()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();
                ViewModel.StopEditingTimeCommand.Execute();

                ViewModel.IsEditingTime.Should().BeFalse();
                ViewModel.IsEditingStartTime.Should().BeFalse();
                ViewModel.IsEditingStopTime.Should().BeFalse();
            }
        }

        public sealed class TheEditedTimeProperty : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void ReturnsTheStartTimeWhenIsEditingStartTime()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();

                ViewModel.EditedTime.Should().Be(parameter.Start);
            }

            [Fact]
            public void ReturnsTheStopTimeWhenIsEditingStopTime()
            {
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();

                ViewModel.EditedTime.Should().Be(parameter.Start + parameter.Duration.Value);
            }

            [Fact]
            public void DoesNotAcceptAnyValueWhenNotEditingNeitherStartNorStopTime()
            {
                var editedValue = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                ViewModel.Prepare(parameter);

                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().NotBe(editedValue);
                ViewModel.StartTime.Should().NotBe(editedValue);
                ViewModel.StopTime.Should().NotBe(editedValue);
            }

            [Fact]
            public void ChangesJustTheStartTime()
            {
                var editedValue = new DateTimeOffset(2018, 01, 07, 0, 0, 0, TimeSpan.Zero);
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();
                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().Be(editedValue);
                ViewModel.StartTime.Should().Be(editedValue);
                ViewModel.StopTime.Should().NotBe(editedValue);
            }

            [Fact]
            public void DoesNotAllowChangingTheStartTimeToMoreThanTheMaximumDate()
            {
                var editedValue = parameter.Start.Add(parameter.Duration.Value).AddHours(1);
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();
                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().Be(ViewModel.MaximumDateTime);
                ViewModel.StartTime.Should().Be(ViewModel.MaximumDateTime);
                ViewModel.StopTime.Should().Be(ViewModel.MaximumDateTime);
            }

            [Fact]
            public void DoesNotAllowChangingTheStartTimeToLessThanTheMinimumDate()
            {
                var editedValue = parameter.Start.AddHours(-1000);
                ViewModel.Prepare(parameter);

                ViewModel.EditStartTimeCommand.Execute();
                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().Be(ViewModel.MinimumDateTime);
                ViewModel.StartTime.Should().Be(ViewModel.MinimumDateTime);
                ViewModel.StopTime.Should().NotBe(ViewModel.MinimumDateTime);
            }

            [Fact]
            public void ChangesJustTheStopTime()
            {
                var editedValue = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();
                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().Be(editedValue);
                ViewModel.StartTime.Should().NotBe(editedValue);
                ViewModel.StopTime.Should().Be(editedValue);
            }

            [Fact]
            public void DoesNotAllowChangingTheStopTimeToMoreThanTheMaximumDate()
            {
                var editedValue = parameter.Start.AddHours(1000);
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();
                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().Be(ViewModel.MaximumDateTime);
                ViewModel.StopTime.Should().Be(ViewModel.MaximumDateTime);
            }

            [Fact]
            public void DoesNotAllowChangingTheStopTimeToLessThanTheMinimumDate()
            {
                var editedValue = parameter.Start.AddHours(-1);
                ViewModel.Prepare(parameter);

                ViewModel.EditStopTimeCommand.Execute();
                ViewModel.EditedTime = editedValue;

                ViewModel.EditedTime.Should().Be(ViewModel.MinimumDateTime);
                ViewModel.StopTime.Should().Be(ViewModel.MinimumDateTime);
            }
        }
    }
}

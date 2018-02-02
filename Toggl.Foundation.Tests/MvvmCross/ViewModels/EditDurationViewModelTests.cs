using System;
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
                => new EditDurationViewModel(NavigationService, TimeService);
        }

        public sealed class TheConstructor : EditDurationViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useNavigationService, bool useTimeService)
            {
                var navigationService = useNavigationService ? NavigationService : null;
                var timeService = useTimeService ? TimeService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new EditDurationViewModel(navigationService, timeService);

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

            [Fact]
            public void SetsTheIsRunningPropertyWhenTheDurationIsNull()
            {
                var start = new DateTimeOffset(2018, 01, 15, 12, 34, 56, TimeSpan.Zero);
                var parameter = DurationParameter.WithStartAndDuration(start, null);

                ViewModel.Prepare(parameter);

                ViewModel.IsRunning.Should().BeTrue();
            }

            [Fact]
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

        public sealed class TheStopTimeEntryCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void StopsARunningTimeEntry()
            {
                var now = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                var runningTEParameter = DurationParameter.WithStartAndDuration(parameter.Start, null);
                ViewModel.Prepare(runningTEParameter);
                TimeService.CurrentDateTime.Returns(now);

                ViewModel.StopTimeEntryCommand.Execute();

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

                ViewModel.StopTimeEntryCommand.Execute();
                subject.OnNext(now.AddSeconds(1));

                ViewModel.StopTime.Should().Be(now);
            }
        }
    }
}

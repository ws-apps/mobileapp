using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class MainViewModelTests
    {
        public abstract class MainViewModelTest : BaseViewModelTests<MainViewModel>
        {
            protected ISubject<SyncProgress> ProgressSubject { get; } = new Subject<SyncProgress>();

            protected IUserPreferences UserPreferences { get; } = Substitute.For<IUserPreferences>();

            protected override MainViewModel CreateViewModel()
            {
                var vm = new MainViewModel(DataSource, TimeService, OnboardingStorage, NavigationService, UserPreferences);
                vm.Prepare();
                return vm;
            }

            protected override void AdditionalSetup()
            {
                base.AdditionalSetup();

                var syncManager = Substitute.For<ISyncManager>();
                syncManager.ProgressObservable.Returns(ProgressSubject.AsObservable());
                DataSource.SyncManager.Returns(syncManager);
            }
        }

        public sealed class TheConstructor : MainViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FiveParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource, 
                                                        bool useTimeService, 
                                                        bool useOnboardingStorage,
                                                        bool useNavigationService,
                                                        bool useUserPreferences)
            {
                var dataSource = useDataSource ? DataSource : null;
                var timeService = useTimeService ? TimeService : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var userPreferences = useUserPreferences ? UserPreferences : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new MainViewModel(dataSource, timeService, onboardingStorage, navigationService, userPreferences);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }

            [Fact, LogIfTooSlow]
            public void IsNotInManualModeByDefault()
            {
                ViewModel.IsInManualMode.Should().BeFalse();
            }
        }

        public sealed class TheViewAppearingMethod : MainViewModelTest
        {
            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public void InitializesTheIsInManualModePropertyAccordingToUsersPreferences(bool isEnabled)
            {
                UserPreferences.IsManualModeEnabled().Returns(isEnabled);

                ViewModel.ViewAppearing();

                ViewModel.IsInManualMode.Should().Be(isEnabled);
            }
        }

        public sealed class TheViewAppearedMethod : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void RequestsTheSuggestionsViewModel()
            {
                ViewModel.ViewAppeared();

                NavigationService.Received().Navigate<SuggestionsViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void RequestsTheLogTimeEntriesViewModel()
            {
                ViewModel.ViewAppeared();

                NavigationService.Received().Navigate<TimeEntriesLogViewModel>();
            }
        }

        public sealed class TheStartTimeEntryCommand : MainViewModelTest
        {
            public TheStartTimeEntryCommand()
            {
                TimeService.CurrentDateTime.Returns(DateTimeOffset.Now);
            }

            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheStartTimeEntryViewModel()
            {
                await ViewModel.StartTimeEntryCommand.ExecuteAsync();

                await NavigationService.Received()
                   .Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(Arg.Any<StartTimeEntryParameters>());
            }

            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheStartTimeEntryViewModelWhenInManualMode()
            {
                ViewModel.IsInManualMode = true;
                await ViewModel.StartTimeEntryCommand.ExecuteAsync();

                await NavigationService.Received()
                   .Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(Arg.Any<StartTimeEntryParameters>());
            }

            [Property]
            public void PassesTheCurrentDateToTheStartTimeEntryViewModel(DateTimeOffset date)
            {
                TimeService.CurrentDateTime.Returns(date);

                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.StartTime == date)
                ).Wait();
            }

            [Fact, LogIfTooSlow]
            public void PassesTheAppropriatePlaceholderToTheStartTimeEntryViewModel()
            {
                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.PlaceholderText == "What are you working on?")
                ).Wait();
            }

            [Fact, LogIfTooSlow]
            public void PassesNullDurationToTheStartTimeEntryViewModel()
            {
                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.Duration.HasValue == false)
                ).Wait();
            }

            [Property]
            public void PassesTheCurrentDateMinusThirtyMinutesToTheStartTimeEntryViewModelWhenInManualMode(DateTimeOffset date)
            {
                TimeService.CurrentDateTime.Returns(date);

                ViewModel.IsInManualMode = true;
                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.StartTime == date.Subtract(TimeSpan.FromMinutes(30)))
                ).Wait();
            }

            [Fact, LogIfTooSlow]
            public void PassesTheAppropriatePlaceholderToTheStartTimeEntryViewModelWhenInManualMode()
            {
                ViewModel.IsInManualMode = true;
                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.PlaceholderText == "What have you done?")
                ).Wait();
            }

            [Fact, LogIfTooSlow]
            public void PassesThirtyMinutesOfDurationToTheStartTimeEntryViewModelWhenInManualMode()
            {
                ViewModel.IsInManualMode = true;
                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received().Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(
                    Arg.Is<StartTimeEntryParameters>(parameter => parameter.Duration.HasValue == true && (int)parameter.Duration.Value.TotalMinutes == 30)
                ).Wait();
            }

            [Fact, LogIfTooSlow]
            public async Task CannotBeExecutedWhenThereIsARunningTimeEntry()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                var observable = Observable.Return(timeEntry);
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);
                ViewModel.Initialize().Wait();

                ViewModel.StartTimeEntryCommand.ExecuteAsync().Wait();

                await NavigationService.DidNotReceive()
                    .Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(Arg.Any<StartTimeEntryParameters>());
            }
        }

        public sealed class TheOpenSettingsCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheSettingsViewModel()
            {
                await ViewModel.OpenSettingsCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<SettingsViewModel>();
            }
        }

        public sealed class TheOpenReportsCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheReportsViewModel()
            {
                const long workspaceId = 10;
                var user = Substitute.For<IDatabaseUser>();
                user.DefaultWorkspaceId.Returns(workspaceId);
                DataSource.User.Current.Returns(Observable.Return(user));

                await ViewModel.OpenReportsCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<ReportsViewModel, long>(workspaceId);
            }
        }

        public sealed class TheStopTimeEntryCommand : MainViewModelTest
        {
            private ISubject<IDatabaseTimeEntry> subject;

            public TheStopTimeEntryCommand()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                subject = new BehaviorSubject<IDatabaseTimeEntry>(timeEntry);
                var observable = subject.AsObservable();
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);

                ViewModel.Initialize().Wait();
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheStopMethodOnTheDataSource()
            {
                var date = DateTimeOffset.UtcNow;
                TimeService.CurrentDateTime.Returns(date);

                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.Received().Stop(date);
            }

            [Fact, LogIfTooSlow]
            public async Task SetsTheElapsedTimeToZero()
            {
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                ViewModel.CurrentTimeEntryElapsedTime.Should().Be(TimeSpan.Zero);
            }

            [Fact, LogIfTooSlow]
            public async Task InitiatesPushSync()
            {
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotInitiatePushSyncWhenSavingFails()
            {
                DataSource.TimeEntries.Stop(Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Throw<IDatabaseTimeEntry>(new Exception()));

                Action stopTimeEntry = () => ViewModel.StopTimeEntryCommand.ExecuteAsync().Wait();

                stopTimeEntry.ShouldThrow<Exception>();
                await DataSource.SyncManager.DidNotReceive().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task CannotBeExecutedTwiceInARowInFastSuccession()
            {
                var taskA = ViewModel.StopTimeEntryCommand.ExecuteAsync();
                var taskB = ViewModel.StopTimeEntryCommand.ExecuteAsync();

                Task.WaitAll(taskA, taskB);

                await DataSource.TimeEntries.Received(1).Stop(Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public async Task CannotBeExecutedTwiceInARow()
            {
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();
                subject.OnNext(null);
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.Received(1).Stop(Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public async Task CannotBeExecutedWhenNoTimeEntryIsRunning()
            {
                subject.OnNext(null);

                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.DidNotReceive().Stop(Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public async Task CanBeExecutedForTheSecondTimeIfAnotherTimeEntryIsStartedInTheMeantime()
            {
                var secondTimeEntry = Substitute.For<IDatabaseTimeEntry>();
                
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();
                subject.OnNext(secondTimeEntry);
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.Received(2).Stop(Arg.Any<DateTimeOffset>());
            }
        }

        public sealed class TheEditTimeEntryCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheEditTimeEntryViewModel()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                var observable = Observable.Return(timeEntry);
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);
                ViewModel.Initialize().Wait();

                await ViewModel.EditTimeEntryCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Navigate<EditTimeEntryViewModel, long>(Arg.Any<long>());
            }

            [Property]
            public void PassesTheCurrentDateToTheStartTimeEntryViewModel(long id)
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                var observable = Observable.Return(timeEntry);
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);
                ViewModel.Initialize().Wait();

                ViewModel.EditTimeEntryCommand.ExecuteAsync().Wait();

                NavigationService.Received()
                    .Navigate<EditTimeEntryViewModel, long>(Arg.Is(id)).Wait();
            }

            [Fact, LogIfTooSlow]
            public async Task CannotBeExecutedWhenThereIsNoRunningTimeEntry()
            {
                var observable = Observable.Return<IDatabaseTimeEntry>(null);
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);
                ViewModel.Initialize().Wait();

                ViewModel.EditTimeEntryCommand.ExecuteAsync().Wait();

                await NavigationService.DidNotReceive()
                    .Navigate<EditTimeEntryViewModel, long>(Arg.Any<long>());
            }
        }

        public abstract class CurrentTimeEntrypropertyTest<T> : MainViewModelTest
        {
            private readonly BehaviorSubject<IDatabaseTimeEntry> currentTimeEntrySubject
                = new BehaviorSubject<IDatabaseTimeEntry>(null);

            protected abstract T ActualValue { get; }
            protected abstract T ExpectedValue { get; }
            protected abstract T ExpectedEmptyValue { get; }

            protected long TimeEntryId = 13;
            protected string Description = "Something";
            protected string Project = "Some project";
            protected string Task = "Some task";
            protected string Client = "Some client";
            protected string ProjectColor = "0000AF";

            private async Task prepare()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(TimeEntryId);
                timeEntry.Description.Returns(Description);
                timeEntry.Project.Name.Returns(Project);
                timeEntry.Project.Color.Returns(ProjectColor);
                timeEntry.Task.Name.Returns(Task);
                timeEntry.Project.Client.Name.Returns(Client);

                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(currentTimeEntrySubject.AsObservable());

                await ViewModel.Initialize();
                currentTimeEntrySubject.OnNext(timeEntry);
            }

            [Fact, LogIfTooSlow]
            public async Task IsSet()
            {
                await prepare();

                ActualValue.Should().Be(ExpectedValue);
            }

            [Fact, LogIfTooSlow]
            public async Task IsUnset()
            {
                await prepare();
                currentTimeEntrySubject.OnNext(null);

                ActualValue.Should().Be(ExpectedEmptyValue);
            }
        }

        public sealed class TheCurrentTimeEntryIdProperty : CurrentTimeEntrypropertyTest<long?>
        {
            protected override long? ActualValue => ViewModel.CurrentTimeEntryId;

            protected override long? ExpectedValue => TimeEntryId;

            protected override long? ExpectedEmptyValue => null;
        }

        public sealed class TheCurrentTimeEntryDescriptionProperty : CurrentTimeEntrypropertyTest<string>
        {
            protected override string ActualValue => ViewModel.CurrentTimeEntryDescription;

            protected override string ExpectedValue => Description;

            protected override string ExpectedEmptyValue => "";
        }

        public sealed class TheCurrentTimeEntryProjectProperty : CurrentTimeEntrypropertyTest<string>
        {
            protected override string ActualValue => ViewModel.CurrentTimeEntryProject;

            protected override string ExpectedValue => Project;

            protected override string ExpectedEmptyValue => "";
        }

        public sealed class TheCurrentTimeEntryProjectColorProperty : CurrentTimeEntrypropertyTest<string>
        {
            protected override string ActualValue => ViewModel.CurrentTimeEntryProjectColor;

            protected override string ExpectedValue => ProjectColor;

            protected override string ExpectedEmptyValue => "";
        }

        public sealed class TheCurrentTimeEntryTaskProperty : CurrentTimeEntrypropertyTest<string>
        {
            protected override string ActualValue => ViewModel.CurrentTimeEntryTask;

            protected override string ExpectedValue => Task;

            protected override string ExpectedEmptyValue => "";
        }

        public sealed class TheCurrentTimeEntryClientProperty : CurrentTimeEntrypropertyTest<string>
        {
            protected override string ActualValue => ViewModel.CurrentTimeEntryClient;

            protected override string ExpectedValue => Client;

            protected override string ExpectedEmptyValue => "";
        }
    }
}

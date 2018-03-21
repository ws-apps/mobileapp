using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Suggestions;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class MainViewModelTests
    {
        public abstract class MainViewModelTest : BaseViewModelTests<MainViewModel>
        {
            protected ISubject<SyncProgress> ProgressSubject { get; } = new Subject<SyncProgress>();

            protected IUserPreferences UserPreferences { get; } = Substitute.For<IUserPreferences>();

            protected TestScheduler Scheduler { get; } = new TestScheduler();

            protected override MainViewModel CreateViewModel()
            {
                var vm = new MainViewModel(Scheduler, DataSource, TimeService, UserPreferences, OnboardingStorage, InteractorFactory, NavigationService);
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
            [ClassData(typeof(SevenParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useScheduler,
                bool useDataSource,
                bool useTimeService,
                bool useUserPreferences,
                bool useOnboardingStorage,
                bool useInteractorFactory,
                bool useNavigationService)
            {
                var scheduler = useScheduler ? Scheduler : null;
                var dataSource = useDataSource ? DataSource : null;
                var timeService = useTimeService ? TimeService : null;
                var userPreferences = useUserPreferences ? UserPreferences : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new MainViewModel(scheduler, dataSource, timeService, userPreferences, onboardingStorage, interactorFactory, navigationService);

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
            public async ThreadingTask NavigatesToTheStartTimeEntryViewModel()
            {
                await ViewModel.StartTimeEntryCommand.ExecuteAsync();

                await NavigationService.Received()
                   .Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(Arg.Any<StartTimeEntryParameters>());
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheStartTimeEntryViewModelWhenInManualMode()
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
            public async ThreadingTask CannotBeExecutedWhenThereIsARunningTimeEntry()
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
            public async ThreadingTask NavigatesToTheSettingsViewModel()
            {
                await ViewModel.OpenSettingsCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<SettingsViewModel>();
            }
        }

        public sealed class TheOpenReportsCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheReportsViewModel()
            {
                const long workspaceId = 10;
                var user = Substitute.For<IDatabaseUser>();
                user.DefaultWorkspaceId.Returns(workspaceId);
                DataSource.User.Current.Returns(Observable.Return(user));

                await ViewModel.OpenReportsCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<ReportsViewModel, long>(workspaceId);
            }
        }

        public class TheStopTimeEntryCommand : MainViewModelTest
        {
            private ISubject<IDatabaseTimeEntry> subject;

            public TheStopTimeEntryCommand()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                subject = new BehaviorSubject<IDatabaseTimeEntry>(timeEntry);
                var observable = subject.AsObservable();
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Returns(observable);

                ViewModel.Initialize().Wait();
                Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CallsTheStopMethodOnTheDataSource()
            {
                var date = DateTimeOffset.UtcNow;
                TimeService.CurrentDateTime.Returns(date);

                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.Received().Stop(date);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask SetsTheElapsedTimeToZero()
            {
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                ViewModel.CurrentTimeEntryElapsedTime.Should().Be(TimeSpan.Zero);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask InitiatesPushSync()
            {
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotInitiatePushSyncWhenSavingFails()
            {
                DataSource.TimeEntries.Stop(Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Throw<IDatabaseTimeEntry>(new Exception()));

                Action stopTimeEntry = () => ViewModel.StopTimeEntryCommand.ExecuteAsync().Wait();

                stopTimeEntry.ShouldThrow<Exception>();
                await DataSource.SyncManager.DidNotReceive().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CannotBeExecutedTwiceInARowInFastSuccession()
            {
                var taskA = ViewModel.StopTimeEntryCommand.ExecuteAsync();
                var taskB = ViewModel.StopTimeEntryCommand.ExecuteAsync();

                ThreadingTask.WaitAll(taskA, taskB);

                await DataSource.TimeEntries.Received(1).Stop(Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CannotBeExecutedTwiceInARow()
            {
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();
                subject.OnNext(null);
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.Received(1).Stop(Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CannotBeExecutedWhenNoTimeEntryIsRunning()
            {
                subject.OnNext(null);
                Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.DidNotReceive().Stop(Arg.Any<DateTimeOffset>());
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CanBeExecutedForTheSecondTimeIfAnotherTimeEntryIsStartedInTheMeantime()
            {
                var secondTimeEntry = Substitute.For<IDatabaseTimeEntry>();

                await ViewModel.StopTimeEntryCommand.ExecuteAsync();
                subject.OnNext(secondTimeEntry);
                Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);
                await ViewModel.StopTimeEntryCommand.ExecuteAsync();

                await DataSource.TimeEntries.Received(2).Stop(Arg.Any<DateTimeOffset>());
            }
        }

        public sealed class TheEditTimeEntryCommand : MainViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheEditTimeEntryViewModel()
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
            public async ThreadingTask CannotBeExecutedWhenThereIsNoRunningTimeEntry()
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

            private async ThreadingTask prepare()
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
                Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask IsSet()
            {
                await prepare();

                ActualValue.Should().Be(ExpectedValue);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask IsUnset()
            {
                await prepare();
                currentTimeEntrySubject.OnNext(null);
                Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(50).Ticks);

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

        public sealed class TheIsWelcomeProperty : MainViewModelTest
        {
            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(28)]
            public async ThreadingTask ReturnsTheSameValueAsTimeEntriesLogViewModel(
                int timeEntryCount)
            {
                var timeEntries = Enumerable
                    .Range(0, timeEntryCount)
                    .Select(createTimeEntry)
                    .ToArray();
                DataSource
                    .TimeEntries
                    .GetAll()
                    .Returns(Observable.Return(timeEntries));
                DataSource
                    .TimeEntries
                    .TimeEntryUpdated
                    .Returns(Observable.Never<(long Id, IDatabaseTimeEntry Entity)>());
                await ViewModel.Initialize();

                ViewModel
                    .IsWelcome
                    .Should()
                    .Be(ViewModel.TimeEntriesLogViewModel.IsWelcome);
            }

            private IDatabaseTimeEntry createTimeEntry(int id)
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                timeEntry.Start.Returns(DateTimeOffset.Now);
                timeEntry.Duration.Returns(100);
                return timeEntry;
            }
        }

        public abstract class InitialStateTest : MainViewModelTest
        {
            protected void PrepareSuggestion()
            {
                DataSource.TimeEntries.IsEmpty.Returns(Observable.Return(false));
                var suggestionProvider = Substitute.For<ISuggestionProvider>();
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(123);
                timeEntry.Start.Returns(DateTimeOffset.Now);
                timeEntry.Duration.Returns((long?)null);
                timeEntry.Description.Returns("something");
                var suggestion = new Suggestion(timeEntry);
                suggestionProvider.GetSuggestions().Returns(Observable.Return(suggestion));
                var providers = new ReadOnlyCollection<ISuggestionProvider>(
                    new List<ISuggestionProvider> { suggestionProvider }
                );
                SuggestionProviderContainer.Providers.Returns(providers);
            }

            protected void PrepareTimeEntry()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(123);
                timeEntry.Start.Returns(DateTimeOffset.Now);
                timeEntry.Duration.Returns(100);
                DataSource
                    .TimeEntries
                    .GetAll()
                    .Returns(Observable.Return(new[] { timeEntry }));
                DataSource
                    .TimeEntries
                    .TimeEntryUpdated
                    .Returns(Observable.Never<(long Id, IDatabaseTimeEntry Entity)>());
            }

            protected void PrepareIsWelcome(bool isWelcome)
            {
                OnboardingStorage.IsNewUser().Returns(isWelcome);
            }
        }

        public sealed class TheShouldShowEmptyStateProperty : InitialStateTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTrueWhenThereAreNoSuggestionsAndNoTimeEntriesAndIsWelcome()
            {
                PrepareIsWelcome(true);
                await ViewModel.Initialize();

                ViewModel.ShouldShowEmptyState.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeSuggestions()
            {
                PrepareSuggestion();

                await ViewModel.Initialize();

                ViewModel.ShouldShowEmptyState.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeTimeEntries()
            {
                PrepareTimeEntry();
                
                await ViewModel.Initialize();

                ViewModel.ShouldShowEmptyState.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenIsNotWelcome()
            {
                PrepareIsWelcome(false);
                await ViewModel.Initialize();

                ViewModel.ShouldShowEmptyState.Should().BeFalse();
            }
        }

        public sealed class TheShouldShowWelcomeBackProperty : InitialStateTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTrueWhenThereAreNoSuggestionsAndNoTimeEntriesAndIsNotWelcome()
            {
                PrepareIsWelcome(false);
                await ViewModel.Initialize();

                ViewModel.ShouldShowWelcomeBack.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeSuggestions()
            {
                PrepareSuggestion();
                await ViewModel.Initialize();

                ViewModel.ShouldShowWelcomeBack.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenThereAreSomeTimeEntries()
            {
                PrepareTimeEntry();
                await ViewModel.Initialize();

                ViewModel.ShouldShowWelcomeBack.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsFalseWhenIsWelcome()
            {
                PrepareIsWelcome(true);
                await ViewModel.Initialize();

                ViewModel.ShouldShowWelcomeBack.Should().BeFalse();
            }
        }

        public sealed class TheInitializeMethod
        {
            public sealed class WhenNavigationActionIsStop : MainViewModelTest
            {
                [Fact, LogIfTooSlow]
                public async ThreadingTask StopsTheCurrentEntry()
                {
                    ViewModel.Init(ApplicationUrls.Main.Action.Stop);
                    await ViewModel.Initialize();

                    await DataSource.TimeEntries.Received().Stop(TimeService.CurrentDateTime);
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask StartsPushSync()
                {
                    ViewModel.Init(ApplicationUrls.Main.Action.Stop);
                    await ViewModel.Initialize();

                    await DataSource.SyncManager.Received().PushSync();
                }
            }

            public sealed class WhenNavigationActionIsContinue : MainViewModelTest
            {
                [Fact, LogIfTooSlow]
                public async ThreadingTask GetsTheContinueMostRecentTimeEntryInteractor()
                {
                    ViewModel.Init(ApplicationUrls.Main.Action.Continue);

                    await ViewModel.Initialize();

                    InteractorFactory.Received().ContinueMostRecentTimeEntry();
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask ExecutesTheContinueMostRecentTimeEntryInteractor()
                {
                    var interactor = Substitute.For<IInteractor<IObservable<IDatabaseTimeEntry>>>();
                    InteractorFactory.ContinueMostRecentTimeEntry().Returns(interactor);
                    ViewModel.Init(ApplicationUrls.Main.Action.Continue);

                    await ViewModel.Initialize();

                    await interactor.Received().Execute();
                }
            }
        }
    }
}

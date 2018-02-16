﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class TimeEntriesLogViewModelTests
    {
        public abstract class TimeEntriesLogViewModelTest : BaseViewModelTests<TimeEntriesLogViewModel>
        {
            protected override TimeEntriesLogViewModel CreateViewModel()
               => new TimeEntriesLogViewModel(DataSource, TimeService, AnalyticsService, OnboardingStorage, NavigationService);
        }

        public sealed class TheConstructor : TimeEntriesLogViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FiveParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource, 
                bool useTimeService, 
                bool useAnalyticsService,
                bool useOnboardingStorage, 
                bool useNavigationService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var timeService = useTimeService ? TimeService : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TimeEntriesLogViewModel(dataSource, timeService, analyticsService, onboardingStorage, navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheEmptyStateTitleProperty : TimeEntriesLogViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTheWelcomeStringIfTheIsNewUserPropertyIsTrue()
            {
                OnboardingStorage.IsNewUser().Returns(true);

                await ViewModel.Initialize();

                ViewModel.EmptyStateTitle.Should().Be(Resources.TimeEntriesLogEmptyStateWelcomeTitle);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTheDefaultStringIfTheIsNewUserPropertyIsFalse()
            {
                OnboardingStorage.IsNewUser().Returns(false);

                await ViewModel.Initialize();

                ViewModel.EmptyStateTitle.Should().Be(Resources.TimeEntriesLogEmptyStateTitle);
            }
        }

        public sealed class TheEmptyStateTextProperty : TimeEntriesLogViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTheWelcomeStringIfTheIsNewUserPropertyIsTrue()
            {
                OnboardingStorage.IsNewUser().Returns(true);

                await ViewModel.Initialize();

                ViewModel.EmptyStateText.Should().Be(Resources.TimeEntriesLogEmptyStateWelcomeText);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ReturnsTheDefaultStringIfTheIsNewUserPropertyIsFalse()
            {
                OnboardingStorage.IsNewUser().Returns(false);

                await ViewModel.Initialize();

                ViewModel.EmptyStateText.Should().Be(Resources.TimeEntriesLogEmptyStateText);
            }
        }

        public sealed class TheTimeEntriesProperty
        {
            [Property]
            public Property ShouldBeOrderedAfterInitialization()
            {
                var generator = ViewModelGenerators.ForTimeEntriesLogViewModel(_ => true).ToArbitrary();
                return Prop.ForAll(generator, viewModel =>
                {
                    viewModel.Initialize().Wait();

                    for (int i = 1; i < viewModel.TimeEntries.Count(); i++)
                    {
                        var dateTime1 = viewModel.TimeEntries.ElementAt(i - 1).Date;
                        var dateTime2 = viewModel.TimeEntries.ElementAt(i).Date;
                        dateTime1.Should().BeAfter(dateTime2);
                    }
                });
            }

            [Property]
            public Property ShouldNotHaveEmptyGroups()
            {
                var generator = ViewModelGenerators.ForTimeEntriesLogViewModel(_ => true).ToArbitrary();
                return Prop.ForAll(generator, viewModel =>
                {
                    viewModel.Initialize().Wait();

                    foreach (var grouping in viewModel.TimeEntries)
                    {
                        grouping.Count().Should().BeGreaterOrEqualTo(1);
                    }
                });
            }

            [Property]
            public Property ShouldHaveOrderedGroupsAfterInitialization()
            {
                var generator =
                    ViewModelGenerators
                        .ForTimeEntriesLogViewModel(m => m == DateTime.UtcNow.Month).ToArbitrary();

                return Prop.ForAll(generator, viewModel =>
                {
                    viewModel.Initialize().Wait();
                    foreach (var grouping in viewModel.TimeEntries)
                    {
                        for (int i = 1; i < grouping.Count(); i++)
                        {
                            var dateTime1 = grouping.ElementAt(i - 1).Start;
                            var dateTime2 = grouping.ElementAt(i).Start;
                            dateTime1.Should().BeOnOrAfter(dateTime2);
                        }
                    }
                });
            }

            public abstract class TimeEntryDataSourceObservableTest : TimeEntriesLogViewModelTest
            {
                private static readonly DateTimeOffset now = new DateTimeOffset(2017, 01, 19, 07, 10, 00, TimeZone.CurrentTimeZone.GetUtcOffset(new DateTime(2017, 01, 19)));

                protected const int InitialAmountOfTimeEntries = 20;
                
                protected Subject<IDatabaseTimeEntry> TimeEntryCreatedSubject = new Subject<IDatabaseTimeEntry>();
                protected Subject<(long Id, IDatabaseTimeEntry Entity)> TimeEntryUpdatedSubject = new Subject<(long, IDatabaseTimeEntry)>();
                protected Subject<long> TimeEntryDeletedSubject = new Subject<long>();
                protected IDatabaseTimeEntry NewTimeEntry =
                    TimeEntry.Builder.Create(21)
                             .SetUserId(10)
                             .SetWorkspaceId(12)
                             .SetDescription("")
                             .SetAt(now)
                             .SetStart(now)
                             .Build();

                protected TimeEntryDataSourceObservableTest()
                {
                    var startTime = now.AddHours(-2);

                    var observable = Enumerable.Range(1, InitialAmountOfTimeEntries)
                        .Select(i => TimeEntry.Builder.Create(i))
                        .Select(builder => builder
                            .SetStart(startTime.AddHours(builder.Id * 2))
                            .SetUserId(11)
                            .SetWorkspaceId(12)
                            .SetDescription("")
                            .SetAt(now)
                            .Build())
                      .Select(te => te.With((long)TimeSpan.FromHours(te.Id * 2 + 2).TotalSeconds))
                      .Apply(Observable.Return);

                    DataSource.TimeEntries.GetAll().Returns(observable);
                    DataSource.TimeEntries.TimeEntryCreated.Returns(TimeEntryCreatedSubject.AsObservable());
                    DataSource.TimeEntries.TimeEntryUpdated.Returns(TimeEntryUpdatedSubject.AsObservable());
                    DataSource.TimeEntries.TimeEntryDeleted.Returns(TimeEntryDeletedSubject.AsObservable());
                }
            }

            public sealed class WhenReceivingAnEventFromTheTimeEntryCreatedObservable : TimeEntryDataSourceObservableTest
            {
                [Fact, LogIfTooSlow]
                public async ThreadingTask AddsTheCreatedTimeEntryToTheList()
                {
                    await ViewModel.Initialize();
                    var newTimeEntry = NewTimeEntry.With((long)TimeSpan.FromHours(1).TotalSeconds);

                    TimeEntryCreatedSubject.OnNext(newTimeEntry);

                    ViewModel.TimeEntries.Any(c => c.Any(te => te.Id == 21)).Should().BeTrue();
                    ViewModel.TimeEntries.Aggregate(0, (acc, te) => acc + te.Count).Should().Be(InitialAmountOfTimeEntries + 1);
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask IgnoresTheTimeEntryIfItsStillRunning()
                {
                    await ViewModel.Initialize();

                    TimeEntryCreatedSubject.OnNext(NewTimeEntry);

                    ViewModel.TimeEntries.Any(c => c.Any(te => te.Id == 21)).Should().BeFalse();
                    ViewModel.TimeEntries.Aggregate(0, (acc, te) => acc + te.Count).Should().Be(InitialAmountOfTimeEntries);
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask SetsTheIsWelcomePropertyToFalse()
                {
                    OnboardingStorage.IsNewUser().Returns(true);
                    await ViewModel.Initialize();

                    TimeEntryCreatedSubject.OnNext(NewTimeEntry.With((long)TimeSpan.FromHours(1).TotalSeconds));

                    ViewModel.IsWelcome.Should().BeFalse();
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask SetsTheUserIsNotNewFlagToFalseInTheStorage()
                {
                    OnboardingStorage.IsNewUser().Returns(true);
                    await ViewModel.Initialize();

                    TimeEntryCreatedSubject.OnNext(NewTimeEntry.With((long)TimeSpan.FromHours(1).TotalSeconds));

                    OnboardingStorage.Received().SetIsNewUser(false);
                }
            }

            public sealed class WhenReceivingAnEventFromTheTimeEntryUpdatedObservable : TimeEntryDataSourceObservableTest
            {
                [Fact, LogIfTooSlow]
                //This can happen, for example, if the time entry was just stopped
                public async ThreadingTask AddsTheTimeEntryIfItWasNotAddedPreviously()
                {
                    await ViewModel.Initialize();
                    var newTimeEntry = NewTimeEntry.With((long)TimeSpan.FromHours(1).TotalSeconds);

                    TimeEntryUpdatedSubject.OnNext((newTimeEntry.Id, newTimeEntry));

                    ViewModel.TimeEntries.Any(c => c.Any(te => te.Id == 21)).Should().BeTrue();
                    ViewModel.TimeEntries.Aggregate(0, (acc, te) => acc + te.Count).Should().Be(InitialAmountOfTimeEntries + 1);
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask IgnoresTheTimeEntryIfItWasDeleted()
                {
                    await ViewModel.Initialize();

                    TimeEntryCreatedSubject.OnNext(NewTimeEntry);

                    ViewModel.TimeEntries.Any(c => c.Any(te => te.Id == 21)).Should().BeFalse();
                    ViewModel.TimeEntries.Aggregate(0, (acc, te) => acc + te.Count).Should().Be(InitialAmountOfTimeEntries);
                }
            }
            
            public sealed class WhenReceivingAnEventFromTheTimeEntryDeletedObservable : TimeEntryDataSourceObservableTest
            {
                [Fact, LogIfTooSlow]
                public async ThreadingTask RemovesTheTimeEntryIfItWasNotRemovedPreviously()
                {
                    await ViewModel.Initialize();
                    var timeEntryCollection = await DataSource.TimeEntries.GetAll().FirstAsync();
                    var timeEntryToDelete = timeEntryCollection.First();

                    TimeEntryDeletedSubject.OnNext(timeEntryToDelete.Id);

                    ViewModel.TimeEntries.All(c => c.All(te => te.Id != timeEntryToDelete.Id)).Should().BeTrue();
                    ViewModel.TimeEntries.Aggregate(0, (acc, te) => acc + te.Count).Should().Be(InitialAmountOfTimeEntries - 1);
                }

                [Fact, LogIfTooSlow]
                public async ThreadingTask RemovesTheWholeCollectionWhenThereAreNoOtherTimeEntriesLeftForThatDay()
                {
                    await ViewModel.Initialize();
                    var timeEntryCollection = ViewModel.TimeEntries.First();
                    var timeEntriesToDelete = new List<TimeEntryViewModel>(timeEntryCollection);
                    var timeEntriesInCollection = timeEntryCollection.Count;

                    foreach (var te in timeEntriesToDelete)
                        TimeEntryDeletedSubject.OnNext(te.Id);

                    ViewModel.TimeEntries.All(c => c.Date != timeEntryCollection.Date).Should().BeTrue();
                    ViewModel.TimeEntries.Aggregate(0, (acc, te) => acc + te.Count).Should().Be(InitialAmountOfTimeEntries - timeEntriesInCollection);
                }
            }
        }

        public sealed class TheEditCommand : TimeEntriesLogViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheEditTimeEntryViewModel()
            {
                var databaseTimeEntry = Substitute.For<IDatabaseTimeEntry>();
                databaseTimeEntry.Duration.Returns(100);
                var timeEntryViewModel = new TimeEntryViewModel(databaseTimeEntry);

                await ViewModel.EditCommand.ExecuteAsync(timeEntryViewModel);

                await NavigationService.Received().Navigate<EditTimeEntryViewModel, long>(
                    Arg.Is<long>(p => p == databaseTimeEntry.Id)
                );
            }
        }

        public sealed class TheContinueTimeEntryCommand : TimeEntriesLogViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask StartsATimeEntry()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Duration.Returns(100);
                var timeEntryViewModel = new TimeEntryViewModel(timeEntry);

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                await DataSource.TimeEntries.Start(Arg.Any<StartTimeEntryDTO>());
            }

            [Property]
            public void StartATimeEntryWithTheSameValuesOfTheSelectedTimeEntry(
                string description,
                long workspaceId,
                long projectId,
                long? taskId,
                bool billable,
                NonNull<long[]> tagIds)
            {
                if (description == null || projectId == 0) return;

                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(projectId);
                project.WorkspaceId.Returns(workspaceId);
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.WorkspaceId.Returns(workspaceId);
                timeEntry.Description.Returns(description);
                timeEntry.Billable.Returns(billable);
                timeEntry.Project.Returns(project);
                timeEntry.TaskId.Returns(taskId);
                timeEntry.TagIds.Returns(tagIds.Get);
                timeEntry.Duration.Returns(100);
                var timeEntryViewModel = new TimeEntryViewModel(timeEntry);

                ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel).Wait();

                DataSource.TimeEntries.Received().Start(Arg.Is<StartTimeEntryDTO>(dto =>
                    dto.WorkspaceId == workspaceId &&
                    dto.TagIds.SequenceEqual(tagIds.Get) &&
                    dto.Description == description &&
                    dto.ProjectId == projectId &&
                    dto.Billable == billable &&
                    dto.TaskId == taskId
                )).Wait();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask InitiatesPushSyncWhenThereIsARunningTimeEntry()
            {
                var timeEntryViewModel = createTimeEntryViewModel();

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask DoesNotInitiatePushSyncWhenStartingFails()
            {
                var timeEntryViewModel = createTimeEntryViewModel();
                DataSource.TimeEntries.Start(Arg.Any<StartTimeEntryDTO>())
                    .Returns(Observable.Throw<IDatabaseTimeEntry>(new Exception()));

                Action executeCommand = () => ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel).Wait();

                executeCommand.ShouldThrow<Exception>();
                await DataSource.SyncManager.DidNotReceive().PushSync();
            }
            
            [Fact, LogIfTooSlow]
            public async ThreadingTask CannotBeExecutedTwiceInARow()
            {
                var timeEntryViewModel = createTimeEntryViewModel();
                DataSource.TimeEntries.Start(Arg.Any<StartTimeEntryDTO>())
                    .Returns(Observable.Never<IDatabaseTimeEntry>());

                var _ = ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);
                var __ = ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                await DataSource.TimeEntries.Received(1).Start(Arg.Any<StartTimeEntryDTO>());
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CanBeExecutedForTheSecondTimeIfStartingTheFirstOneFinishesSuccessfully()
            {
                var timeEntryViewModel = createTimeEntryViewModel();
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                DataSource.TimeEntries.Start(Arg.Any<StartTimeEntryDTO>())
                    .Returns(Observable.Return(timeEntry));

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);
                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                await DataSource.TimeEntries.Received(2).Start(Arg.Any<StartTimeEntryDTO>());
            }

            [Fact]
            public async ThreadingTask RegistersTheEventInTheAnalyticsService()
            {
                var timeEntryViewModel = createTimeEntryViewModel();

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                AnalyticsService.Received().TrackStartedTimeEntry(TimeEntryStartOrigin.Continue);
            }

            private TimeEntryViewModel createTimeEntryViewModel()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Duration.Returns(100);
                return new TimeEntryViewModel(timeEntry);
            }
        }

        public sealed class TheDeleteCommand : TimeEntriesLogViewModelTest
        {
            [Property]
            public void DeletesTheTimeEntry(long id)
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                timeEntry.Duration.Returns(100);
                var timeEntryViewModel = new TimeEntryViewModel(timeEntry);

                ViewModel.DeleteCommand.ExecuteAsync(timeEntryViewModel).Wait();

                DataSource.TimeEntries.Received().Delete(id).Wait();
            }
        }
    }
}

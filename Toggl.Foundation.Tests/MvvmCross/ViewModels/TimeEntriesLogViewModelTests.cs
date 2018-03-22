﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
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
                => new TimeEntriesLogViewModel(TimeService, DataSource, InteractorFactory, OnboardingStorage, NavigationService);
        }

        public sealed class TheConstructor : TimeEntriesLogViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FiveParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource,
                bool useTimeService,
                bool useInteractorFactory,
                bool useOnboardingStorage,
                bool useNavigationService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var timeService = useTimeService ? TimeService : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TimeEntriesLogViewModel(timeService, dataSource, interactorFactory, onboardingStorage, navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheTimeEntriesProperty : TimeEntriesLogViewModelTest
        {
            [Property]
            public Property ShouldBeOrderedAfterInitialization()
            {
                var arb = generatorForTimeEntriesLogViewModel(_ => true).ToArbitrary();
                return Prop.ForAll(arb, viewModel =>
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
                var arb = generatorForTimeEntriesLogViewModel(_ => true).ToArbitrary();
                return Prop.ForAll(arb, viewModel =>
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
                var arb =
                    generatorForTimeEntriesLogViewModel(m => m == DateTime.UtcNow.Month).ToArbitrary();

                return Prop.ForAll(arb, (Action<TimeEntriesLogViewModel>)(viewModel =>
                {
                    viewModel.Initialize().Wait();
                    foreach (var grouping in viewModel.TimeEntries)
                    {
                        for (int i = 1; i < grouping.Count(); i++)
                        {
                            var dateTime1 = grouping.ElementAt(i - 1).StartTime;
                            var dateTime2 = grouping.ElementAt(i).StartTime;
                            AssertionExtensions.Should(dateTime1).BeOnOrAfter((DateTimeOffset)dateTime2);
                        }
                    }
                }));
            }

            private Gen<TimeEntriesLogViewModel> generatorForTimeEntriesLogViewModel(Func<int, bool> filter)
            {
                var now = new DateTimeOffset(2017, 08, 13, 08, 01, 23, TimeSpan.Zero);
                var monthsGenerator = Gen.Choose(1, 12).Where(filter);
                var yearGenerator = Gen.Choose(2007, now.Year);

                return Arb.Default
                    .Array<DateTimeOffset>()
                    .Generator
                    .Select(dateTimes =>
                    {
                        var viewModel = CreateViewModel();
                        var year = yearGenerator.Sample(0, 1).First();

                        var observable = dateTimes
                            .Select(newDateWithGenerator(monthsGenerator, year))
                            .Select(d => TimeEntry.Builder
                                    .Create(-1)
                                    .SetUserId(-2)
                                    .SetWorkspaceId(-3)
                                    .SetStart(d)
                                    .SetDescription("")
                                    .SetAt(now).Build())
                            .Apply(Observable.Return);

                        DataSource.TimeEntries.GetAll().Returns(observable);

                        return viewModel;
                    });
            }

            private static Func<DateTimeOffset, DateTimeOffset> newDateWithGenerator(Gen<int> monthGenerator, int year)
            {
                var month = monthGenerator.Sample(0, 1).First();
                var day = Gen.Choose(1, DateTime.DaysInMonth(year, month)).Sample(0, 1).First();

                return dateTime =>
                    new DateTime(year, month, day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            }
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
                var observable = Observable.Return(true);
                OnboardingStorage.IsNewUser.Returns(observable);
                await ViewModel.Initialize();

                TimeEntryCreatedSubject.OnNext(NewTimeEntry.With((long)TimeSpan.FromHours(1).TotalSeconds));

                ViewModel.IsWelcome.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask SetsTheUserIsNotNewFlagToFalseInTheStorage()
            {
                var observable = Observable.Return(true);
                OnboardingStorage.IsNewUser.Returns(observable);
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

        public sealed class TheEditCommand : TimeEntriesLogViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NavigatesToTheEditTimeEntryViewModel()
            {
                var databaseTimeEntry = Substitute.For<IDatabaseTimeEntry>();
                databaseTimeEntry.Duration.Returns(100);
                var timeEntryViewModel = new TimeEntryViewModel(databaseTimeEntry, DurationFormat.Improved);

                await ViewModel.EditCommand.ExecuteAsync(timeEntryViewModel);

                await NavigationService.Received().Navigate<EditTimeEntryViewModel, long>(
                    Arg.Is<long>(p => p == databaseTimeEntry.Id)
                );
            }
        }

        public sealed class TheContinueTimeEntryCommand : TimeEntriesLogViewModelTest
        {
            public TheContinueTimeEntryCommand()
            {
                var user = Substitute.For<IDatabaseUser>();
                user.Id.Returns(10);
                DataSource.User.Current.Returns(Observable.Return(user));

                TimeService.CurrentDateTime.Returns(DateTimeOffset.Now);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CallsTheContinueTimeEntryInteractor()
            {
                var timeEntryViewModel = createTimeEntryViewModel();

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                InteractorFactory.Received().ContinueTimeEntry(timeEntryViewModel);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask ExecutesTheContinueTimeEntryInteractor()
            {
                var mockedInteractor = Substitute.For<IInteractor<IObservable<IDatabaseTimeEntry>>>();
                InteractorFactory.ContinueTimeEntry(Arg.Any<ITimeEntryPrototype>()).Returns(mockedInteractor);
                var timeEntryViewModel = createTimeEntryViewModel();

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                await mockedInteractor.Received().Execute();
            }

            [Fact, LogIfTooSlow]
            public void CannotBeExecutedTwiceInARow()
            {
                var timeEntryViewModel = createTimeEntryViewModel();
                var mockedInteractor = Substitute.For<IInteractor<IObservable<IDatabaseTimeEntry>>>();
                InteractorFactory.ContinueTimeEntry(Arg.Any<ITimeEntryPrototype>()).Returns(mockedInteractor);
                mockedInteractor.Execute()
                    .Returns(Observable.Never<IDatabaseTimeEntry>());

                ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);
                ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                InteractorFactory.Received(1).ContinueTimeEntry(timeEntryViewModel);
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask CanBeExecutedForTheSecondTimeIfStartingTheFirstOneFinishesSuccessfully()
            {
                var timeEntryViewModel = createTimeEntryViewModel();
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                var mockedInteractor = Substitute.For<IInteractor<IObservable<IDatabaseTimeEntry>>>();
                InteractorFactory.ContinueTimeEntry(Arg.Any<ITimeEntryPrototype>()).Returns(mockedInteractor);
                mockedInteractor.Execute()
                    .Returns(Observable.Return(timeEntry));

                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);
                await ViewModel.ContinueTimeEntryCommand.ExecuteAsync(timeEntryViewModel);

                InteractorFactory.Received(2).ContinueTimeEntry(timeEntryViewModel);
            }

            private TimeEntryViewModel createTimeEntryViewModel()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Duration.Returns(100);
                timeEntry.WorkspaceId.Returns(10);
                return new TimeEntryViewModel(timeEntry, DurationFormat.Improved);
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
                timeEntry.WorkspaceId.Returns(10);
                var timeEntryViewModel = new TimeEntryViewModel(timeEntry, DurationFormat.Improved);

                ViewModel.DeleteCommand.ExecuteAsync(timeEntryViewModel).Wait();

                DataSource.TimeEntries.Received().Delete(id).Wait();
            }
        }
    }
}

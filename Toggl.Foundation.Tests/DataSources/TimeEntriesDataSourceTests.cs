﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Exceptions;
using Toggl.PrimeRadiant.Models;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.DataSources
{
    public sealed class TimeEntriesDataSourceTests
    {
        public abstract class TimeEntryDataSourceTest : InteractorAwareTests
        {
            protected const long ProjectId = 10;

            protected const long WorkspaceId = 11;

            protected const long UserId = 12;

            protected const long CurrentRunningId = 13;

            protected const long TaskId = 14;

            protected ITimeEntriesSource TimeEntriesSource { get; }

            protected TestScheduler TestScheduler { get; } = new TestScheduler();

            protected DateTimeOffset ValidTime { get; } = DateTimeOffset.UtcNow;

            protected IDatabaseTimeEntry DatabaseTimeEntry { get; } =
                TimeEntry.Builder
                    .Create(CurrentRunningId)
                    .SetUserId(UserId)
                    .SetDescription("")
                    .SetWorkspaceId(WorkspaceId)
                    .SetSyncStatus(SyncStatus.InSync)
                    .SetAt(DateTimeOffset.UtcNow.AddDays(-1))
                    .SetStart(DateTimeOffset.UtcNow.AddHours(-2))
                    .Build();

            protected IRepository<IDatabaseTimeEntry> Repository { get; } = Substitute.For<IRepository<IDatabaseTimeEntry>>();

            protected TimeEntryDataSourceTest()
            {
                TimeEntriesSource = new TimeEntriesDataSource(IdProvider, Repository, TimeService);

                IdProvider.GetNextIdentifier().Returns(-1);
                Repository.GetById(Arg.Is(DatabaseTimeEntry.Id)).Returns(Observable.Return(DatabaseTimeEntry));

                Repository.Create(Arg.Any<IDatabaseTimeEntry>())
                          .Returns(info => Observable.Return(info.Arg<IDatabaseTimeEntry>()));

                Repository.Update(Arg.Any<long>(), Arg.Any<IDatabaseTimeEntry>())
                          .Returns(info => Observable.Return(info.Arg<IDatabaseTimeEntry>()));
            }
        }

        public sealed class TheConstructor : TimeEntryDataSourceTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useIdProvider,
                bool useRepository,
                bool useTimeService)
            {
                var idProvider = useIdProvider ? IdProvider : null;
                var repository = useRepository ? Repository : null;
                var timeService = useTimeService ? TimeService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new TimeEntriesDataSource(idProvider, repository, timeService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheGetAllMethod : TimeEntryDataSourceTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask NeverReturnsDeletedTimeEntries()
            {
                var result = Enumerable
                    .Range(0, 20)
                    .Select(i =>
                    {
                        var isDeleted = i % 2 == 0;
                        var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                        timeEntry.Id.Returns(i);
                        timeEntry.IsDeleted.Returns(isDeleted);
                        return timeEntry;
                    });
                Repository
                    .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(callInfo =>
                        Observable
                             .Return(result)
                             .Select(x => x.Where(callInfo.Arg<Func<IDatabaseTimeEntry, bool>>())));

                var timeEntries = await TimeEntriesSource.GetAll(x => x.Id > 10);

                timeEntries.Should().HaveCount(5);
            }
        }

        public sealed class TheStopMethod : TimeEntryDataSourceTest
        {
            public TheStopMethod()
            {
                long duration = (long)(DateTimeOffset.UtcNow - DatabaseTimeEntry.Start).TotalSeconds;
                var timeEntries = new List<IDatabaseTimeEntry>
                {
                    DatabaseTimeEntry,
                    DatabaseTimeEntry.With(duration),
                    DatabaseTimeEntry.With(duration),
                    DatabaseTimeEntry.With(duration)
                };

                Repository
                    .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(callInfo =>
                        Observable
                             .Return(timeEntries)
                             .Select(x => x.Where(callInfo.Arg<Func<IDatabaseTimeEntry, bool>>())));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheTimeEntrySettingItsDuration()
            {
                await TimeEntriesSource.Stop(ValidTime); 
                await Repository.Received().Update(Arg.Any<long>(), Arg.Is<IDatabaseTimeEntry>(te => te.Duration == (long)(ValidTime - te.Start).TotalSeconds));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheTimeEntryMakingItSyncNeeded()
            {
                await TimeEntriesSource.Stop(ValidTime);

                await Repository.Received().Update(Arg.Any<long>(), Arg.Is<IDatabaseTimeEntry>(te => te.SyncStatus == SyncStatus.SyncNeeded));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask SetsTheCurrentlyRunningTimeEntryToNull()
            {
                var observer = TestScheduler.CreateObserver<ITimeEntry>();
                TimeEntriesSource.CurrentlyRunningTimeEntry.Subscribe(observer);
                Repository.Update(Arg.Any<long>(), Arg.Any<IDatabaseTimeEntry>()).Returns(callInfo => Observable.Return(callInfo.Arg<IDatabaseTimeEntry>()));

                await TimeEntriesSource.Stop(ValidTime);

                observer.Messages.Single().Value.Value.Should().BeNull();
            }

            [Fact, LogIfTooSlow]
            public void ThrowsIfThereAreNoRunningTimeEntries()
            {
                long duration = (long)(DateTimeOffset.UtcNow - DatabaseTimeEntry.Start).TotalSeconds;
                var timeEntries = new List<IDatabaseTimeEntry>
                {
                    DatabaseTimeEntry.With(duration),
                    DatabaseTimeEntry.With(duration),
                    DatabaseTimeEntry.With(duration)
                };

                Repository
                    .GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(callInfo =>
                        Observable
                             .Return(timeEntries)
                             .Select(x => x.Where(callInfo.Arg<Func<IDatabaseTimeEntry, bool>>())));

                var observer = TestScheduler.CreateObserver<ITimeEntry>();
                var observable = TimeEntriesSource.Stop(ValidTime);
                observable.Subscribe(observer);

                observer.Messages.Single().Value.Exception.Should().BeOfType<NoRunningTimeEntryException>();
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask EmitsANewEventOnTheTimeEntryUpdatedObservable()
            {
                var observer = TestScheduler.CreateObserver<(long Id, IDatabaseTimeEntry Entity)>();
                TimeEntriesSource.TimeEntryUpdated.Subscribe(observer);

                await TimeEntriesSource.Stop(ValidTime);

                var tuple = observer.Messages.Single().Value.Value;
                tuple.Id.Should().Be(CurrentRunningId);
                tuple.Entity.Duration.Should().Be((long)(ValidTime - tuple.Entity.Start).TotalSeconds);
            }
        } 
        public sealed class TheDeleteMethod : TimeEntryDataSourceTest
        {
            [Fact, LogIfTooSlow]
            public async ThreadingTask SetsTheDeletedFlag()
            {
                await TimeEntriesSource.Delete(DatabaseTimeEntry.Id).LastOrDefaultAsync();

                await Repository.Received().Update(Arg.Is(DatabaseTimeEntry.Id), Arg.Is<IDatabaseTimeEntry>(te => te.IsDeleted == true));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask SetsTheSyncNeededStatus()
            {
                await TimeEntriesSource.Delete(DatabaseTimeEntry.Id).LastOrDefaultAsync();

                await Repository.Received().Update(Arg.Is(DatabaseTimeEntry.Id), Arg.Is<IDatabaseTimeEntry>(te => te.SyncStatus == SyncStatus.SyncNeeded));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheCorrectTimeEntry()
            {
                await TimeEntriesSource.Delete(DatabaseTimeEntry.Id).LastOrDefaultAsync();

                await Repository.Received().GetById(Arg.Is(DatabaseTimeEntry.Id));
                await Repository.Received().Update(Arg.Is(DatabaseTimeEntry.Id), Arg.Is<IDatabaseTimeEntry>(te => te.Id == DatabaseTimeEntry.Id));
            }

            [Fact, LogIfTooSlow]
            public void EmitsSingleElementBeforeCompleting()
            {
                var observer = Substitute.For<IObserver<Unit>>();

                TimeEntriesSource.Delete(DatabaseTimeEntry.Id).Subscribe(observer);

                observer.Received(1).OnNext(Arg.Any<Unit>());
                observer.Received(1).OnCompleted();
            }

            [Fact, LogIfTooSlow]
            public void PropagatesErrorIfUpdateFails()
            {
                var timeEntry = TimeEntry.Builder.Create(12)
                      .SetStart(DateTimeOffset.UtcNow)
                      .SetSyncStatus(SyncStatus.InSync)
                      .SetDescription("")
                      .SetUserId(11)
                      .SetWorkspaceId(10)
                      .SetAt(DateTimeOffset.UtcNow)
                      .Build();

                var timeEntryObservable = Observable.Return(timeEntry);
                var errorObservable = Observable.Throw<IDatabaseTimeEntry>(new EntityNotFoundException(new Exception()));
                Repository.GetById(Arg.Is(timeEntry.Id)).Returns(timeEntryObservable);
                Repository.Update(Arg.Any<long>(), Arg.Any<IDatabaseTimeEntry>()).Returns(errorObservable);
                var observer = Substitute.For<IObserver<Unit>>();

                TimeEntriesSource.Delete(timeEntry.Id).Subscribe(observer);

                observer.Received().OnError(Arg.Any<EntityNotFoundException>());
            }

            [Fact, LogIfTooSlow]
            public void PropagatesErrorIfTimeEntryIsNotInRepository()
            {
                var observer = Substitute.For<IObserver<Unit>>();
                Repository.GetById(Arg.Any<long>())
                          .Returns(Observable.Throw<IDatabaseTimeEntry>(new EntityNotFoundException(new Exception())));

                TimeEntriesSource.Delete(12).Subscribe(observer);

                observer.Received().OnError(Arg.Any<EntityNotFoundException>());
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask RemovesCurrentlyRunningTimeEntryWhenItIsDeleted()
            {
                DataSource.TimeEntries.Returns(TimeEntriesSource);
                var runningTimeEntriesHistory = new List<IDatabaseTimeEntry>();
                var user = Substitute.For<IDatabaseUser>();
                user.Id.Returns(10);
                DataSource.User.Current.Returns(Observable.Return(user));
                TimeEntriesSource.CurrentlyRunningTimeEntry
                    .Subscribe(te => runningTimeEntriesHistory.Add(te));
                var prototype = new MockTimeEntryPrototype
                {
                    WorkspaceId = WorkspaceId,
                    StartTime = ValidTime,
                    Description = "Some description",
                    IsBillable = true,
                    ProjectId = ProjectId
                };
                prepareBatchUpdate();
                var timeEntry = await InteractorFactory.CreateTimeEntry(prototype).Execute();
                Repository.GetById(Arg.Is(timeEntry.Id)).Returns(Observable.Return(timeEntry));

                TimeEntriesSource.Delete(timeEntry.Id).Wait();

                runningTimeEntriesHistory.Should().HaveCount(3);
                runningTimeEntriesHistory[0].Should().Be(null); // originally there is no running time entry (in the repository)
                runningTimeEntriesHistory[1].Id.Should().Be(timeEntry.Id);
                runningTimeEntriesHistory[2].Should().Be(null);
            }

            private void prepareBatchUpdate()
            {
                Repository.BatchUpdate(
                        Arg.Any<IEnumerable<(long, IDatabaseTimeEntry)>>(),
                        Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                        Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>())
                    .Returns(info => Observable.Return(new[] { new CreateResult<IDatabaseTimeEntry>(info.Arg<IEnumerable<(long, IDatabaseTimeEntry Entity)>>().First().Entity) }));
            }
        }

        public sealed class TheUpdateMethod : TimeEntryDataSourceTest
        {
            private EditTimeEntryDto prepareTest()
            {
                var observable = Observable.Return(DatabaseTimeEntry);
                Repository.GetById(Arg.Is(DatabaseTimeEntry.Id)).Returns(observable);
                return new EditTimeEntryDto
                {
                    Id = DatabaseTimeEntry.Id,
                    Description = "New description",
                    StartTime = DateTimeOffset.UtcNow,
                    ProjectId = 13,
                    Billable = true,
                    WorkspaceId = 71,
                    TagIds = new long[] { 1, 10, 34, 42 }
                };
            }

            private bool ensurePropertiesDidNotChange(IDatabaseTimeEntry timeEntry)
                => timeEntry.Id == DatabaseTimeEntry.Id
                && timeEntry.UserId == DatabaseTimeEntry.UserId
                && timeEntry.IsDeleted == DatabaseTimeEntry.IsDeleted
                && timeEntry.ServerDeletedAt == DatabaseTimeEntry.ServerDeletedAt;

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheDescriptionProperty()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => te.Description == dto.Description));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheSyncStatusProperty()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => te.SyncStatus == SyncStatus.SyncNeeded));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheAtProperty()
            {
                var dto = prepareTest();
                TimeService.CurrentDateTime.Returns(DateTimeOffset.UtcNow);

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => te.At > DatabaseTimeEntry.At));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheProjectId()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => te.ProjectId == dto.ProjectId));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheBillbaleFlag()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => te.Billable == dto.Billable));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheTagIds()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => te.TagIds.SequenceEqual(dto.TagIds)));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UpdatesTheWorkspaceId()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(
                    Arg.Is(dto.Id),
                    Arg.Is<IDatabaseTimeEntry>(te => te.WorkspaceId == dto.WorkspaceId));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask LeavesAllOtherPropertiesUnchanged()
            {
                var dto = prepareTest();

                await TimeEntriesSource.Update(dto);

                await Repository.Received().Update(Arg.Is(dto.Id), Arg.Is<IDatabaseTimeEntry>(te => ensurePropertiesDidNotChange(te)));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask NotifiesAboutTheUpdate()
            {
                var observable = Observable.Return(DatabaseTimeEntry);
                Repository.GetById(Arg.Is(DatabaseTimeEntry.Id)).Returns(observable);
                var dto = new EditTimeEntryDto { Id = DatabaseTimeEntry.Id, Description = "New description", StartTime = DateTimeOffset.UtcNow, WorkspaceId = 71 };
                var observer = Substitute.For<IObserver<(long, IDatabaseTimeEntry)>>();
                TimeEntriesSource.TimeEntryUpdated.Subscribe(observer);

                await TimeEntriesSource.Update(dto);

                observer.Received().OnNext(Arg.Is<(long Id, IDatabaseTimeEntry)>(te => te.Id == dto.Id));
            }

            [Fact, LogIfTooSlow]
            public async ThreadingTask UsesTheUpdatedEntityFromTheRepositoryAndNotTheArgumentToUpdateTheCurrentlyRunningTimeEntry()
            {
                var observable = Observable.Return(DatabaseTimeEntry);
                Repository.GetById(Arg.Is(DatabaseTimeEntry.Id)).Returns(observable);
                var updatedTimeEntry = Substitute.For<IDatabaseTimeEntry>();
                updatedTimeEntry.Id.Returns(123);
                Repository.Update(DatabaseTimeEntry.Id, Arg.Any<IDatabaseTimeEntry>()).Returns(Observable.Return(updatedTimeEntry));
                var dto = new EditTimeEntryDto { Id = DatabaseTimeEntry.Id, Description = "New description", StartTime = DateTimeOffset.UtcNow, WorkspaceId = 71 };
                var observer = Substitute.For<IObserver<(long, IDatabaseTimeEntry)>>();
                TimeEntriesSource.TimeEntryUpdated.Subscribe(observer);

                await TimeEntriesSource.Update(dto);

                observer.Received().OnNext(Arg.Is<(long, IDatabaseTimeEntry Entity)>(te =>
                    te.Entity.Id == updatedTimeEntry.Id));
            }
        }
    }
}

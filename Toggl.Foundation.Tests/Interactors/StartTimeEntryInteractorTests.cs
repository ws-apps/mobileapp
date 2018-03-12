using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Xunit;
using ITimeEntryPrototype = Toggl.Foundation.Models.ITimeEntryPrototype;

namespace Toggl.Foundation.Tests.Interactors
{
    public sealed class StartTimeEntryInteractorTests
    {
        public abstract class StartTimeEntryInteractorTest : InteractorAwareTests
        {
            protected IRepository<IDatabaseTimeEntry> Repository { get; } = Substitute.For<IRepository<IDatabaseTimeEntry>>();

            protected const long ProjectId = 10;

            protected const long WorkspaceId = 11;

            protected const long UserId = 12;

            protected const long TaskId = 14;

            protected TestScheduler TestScheduler { get; } = new TestScheduler();

            protected string ValidDescription { get; } = "Testing software";

            protected DateTimeOffset ValidTime { get; } = DateTimeOffset.UtcNow;

            protected ITimeEntryPrototype CreatePrototype(
                DateTimeOffset startTime,
                string description,
                bool billable,
                long? projectId,
                long? taskId = null,
                TimeSpan? duration = null) => new MockTimeEntryPrototype
                {
                    TaskId = taskId,
                    WorkspaceId = WorkspaceId,
                    StartTime = startTime,
                    Description = description,
                    IsBillable = billable,
                    ProjectId = projectId,
                    Duration = duration
                };

            protected StartTimeEntryInteractorTest()
            {
                var user = Substitute.For<IDatabaseUser>();
                user.Id.Returns(UserId);
                DataSource.User.Current.Returns(Observable.Return(user));

                Repository.BatchUpdate(
                    Arg.Any<IEnumerable<(long, IDatabaseTimeEntry)>>(),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>())
                    .Returns(info => Observable.Return(new[] { new CreateResult<IDatabaseTimeEntry>(info.Arg<IEnumerable<(long, IDatabaseTimeEntry Entity)>>().First().Entity) }));
            }

            protected abstract IObservable<IDatabaseTimeEntry> CallInteractor(ITimeEntryPrototype prototype);

            [Fact, LogIfTooSlow]
            public async Task CreatesANewTimeEntryInTheDatabase()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await Repository.ReceivedWithAnyArgs().BatchUpdate(null, null, null);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesASyncNeededTimeEntry()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await Repository.Received().BatchUpdate(
                    Arg.Is<IEnumerable<(long, IDatabaseTimeEntry Entity)>>(enumerable => enumerable.First().Entity.SyncStatus == SyncStatus.SyncNeeded),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>());
            }

            [Theory, LogIfTooSlow]
            [InlineData(true)]
            [InlineData(false)]
            public async Task CreatesATimeEntryWithTheProvidedValueForBillable(bool billable)
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, billable, ProjectId));

                await batchUpdateCalledWith(te => te.Billable == billable);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithTheProvidedValueForDescription()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await batchUpdateCalledWith(te => te.Description == ValidDescription);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithTheProvidedValueForStartTime()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await batchUpdateCalledWith(te => te.Start == ValidTime);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithTheProvidedValueForProjectId()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await batchUpdateCalledWith(te => te.ProjectId == ProjectId);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithTheProvidedValueForUserId()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await batchUpdateCalledWith(te => te.UserId == UserId);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithTheProvidedValueForTaskId()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId, TaskId));

                await batchUpdateCalledWith(te => te.TaskId == TaskId);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithTheProvidedValueForWorkspaceId()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await batchUpdateCalledWith(te => te.WorkspaceId == WorkspaceId);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesATimeEntryWithAnIdProvidedByTheIdProvider()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                await batchUpdateCalledWith(te => te.Id == -1);
            }

            [Fact, LogIfTooSlow]
            public async Task SetstheCreatedTimeEntryAsTheCurrentlyRunningTimeEntry()
            {
                var observer = TestScheduler.CreateObserver<ITimeEntry>();
                DataSource.TimeEntries.CurrentlyRunningTimeEntry.Where(te => te != null).Subscribe(observer);

                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                var currentlyRunningTimeEntry = observer.Messages.Single().Value.Value;
                await batchUpdateCalledWith(te => te.Start == currentlyRunningTimeEntry.Start);
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesAStoppedTimeEntry()
            {
                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId, duration: TimeSpan.FromSeconds(5)));

                await batchUpdateCalledWith(te => te.Duration.HasValue);
            }

            [Fact, LogIfTooSlow]
            public async Task EmitsANewEventOnTheTimeEntryCreatedObservable()
            {
                var observer = TestScheduler.CreateObserver<ITimeEntry>();
                DataSource.TimeEntries.TimeEntryCreated.Subscribe(observer);

                await CallInteractor(CreatePrototype(ValidTime, ValidDescription, true, ProjectId));

                observer.Messages.Single().Value.Value.Id.Should().Be(-1);
                observer.Messages.Single().Value.Value.Start.Should().Be(ValidTime);
            }

            private async Task batchUpdateCalledWith(Predicate<IDatabaseTimeEntry> predicate)
            {
                await Repository.Received().BatchUpdate(
                    Arg.Is<IEnumerable<(long, IDatabaseTimeEntry Entity)>>(enumerable => predicate(enumerable.First().Entity)),
                    Arg.Any<Func<IDatabaseTimeEntry, IDatabaseTimeEntry, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<IDatabaseTimeEntry>>());
            }

            [Fact, LogIfTooSlow]
            public async Task NotifiesShortcutCreatorAboutNewEntry()
            {
                var dto = CreatePrototype(ValidTime, ValidDescription, true, ProjectId, TaskId);
                var timeEntry = await CallInteractor(dto);

                ApplicationShortcutCreator
                    .Received()
                    .OnTimeEntryStarted(
                        Arg.Is<ITimeEntry>(te =>
                            te.Description == dto.Description
                            && te.Start == dto.StartTime
                            && te.Billable == dto.IsBillable
                            && te.ProjectId == dto.ProjectId
                            && te.TaskId == dto.TaskId));
            }
        }
    }
}

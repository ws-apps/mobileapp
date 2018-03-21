﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Interactors.Shared;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors
{
    public sealed class ContinueMostRecentTimeEntryInteractorTests
    {
        public sealed class TheConstructor : BaseInteractorTests
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FourParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useIdProvider,
                bool useDataSource,
                bool useTimeService,
                bool useAnalyticsService)
            {
                var idProvider = useIdProvider ? IdProvider : null;
                var dataSource = useDataSource ? DataSource : null;
                var timeService = useTimeService ? TimeService : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new ContinueMostRecentTimeEntryInteractor(idProvider, timeService, dataSource, analyticsService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private readonly ContinueMostRecentTimeEntryInteractor interactor;
            private readonly IDatabaseTimeEntry mostRecentTimeEntry;
            private readonly DateTimeOffset now;

            public TheExecuteMethod()
            {
                interactor = new ContinueMostRecentTimeEntryInteractor(
                    IdProvider, TimeService, DataSource, AnalyticsService);

                now = new DateTimeOffset(1973, 1, 2, 3, 4, 5, TimeSpan.Zero);
                TimeService.CurrentDateTime.Returns(now);

                var timeEntries = Enumerable
                    .Range(0, 10)
                    .Select(i => new Mocks.MockTimeEntry
                    {
                        Id = i,
                        UserId = i + 10,
                        TaskId = i + 20,
                        Duration = i + 30,
                        ProjectId = i + 40,
                        WorkspaceId = i + 50,
                        Billable = i % 2 == 0,
                        Description = i.ToString(),
                        Start = DateTimeOffset.Now.AddHours(i),
                        TagIds = new long[] { 100, 101, 100 + i }
                    });
                mostRecentTimeEntry = timeEntries.Last();
                DataSource.TimeEntries.GetAll().Returns(Observable.Return(timeEntries));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentTagIds()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.TagIds.SequenceEqual(mostRecentTimeEntry.TagIds)));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentUserId()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.UserId == mostRecentTimeEntry.UserId));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentTaskId()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.TaskId == mostRecentTimeEntry.TaskId));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentBillable()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.Billable == mostRecentTimeEntry.Billable));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentProjectId()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.ProjectId == mostRecentTimeEntry.ProjectId));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryAtCurrentDateTime()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.At == now));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithSyncNeeded()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.SyncStatus == SyncStatus.SyncNeeded));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentDescription()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.Description == mostRecentTimeEntry.Description));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithCurrentStartTime()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.Start == now));
            }

            [Fact, LogIfTooSlow]
            public async Task CreatesTimeEntryWithMostRecentWorkspaceId()
            {
                await interactor.Execute();

                await DataSource
                    .TimeEntries
                    .Received()
                    .Create(Arg.Is<IDatabaseTimeEntry>(
                        te => te.WorkspaceId == mostRecentTimeEntry.WorkspaceId));
            }

            [Fact, LogIfTooSlow]
            public async Task InitiatesPushSync()
            {
                await interactor.Execute();

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task TracksStartedTimeEntry()
            {
                await interactor.Execute();

                AnalyticsService.Received().TrackStartedTimeEntry(TimeEntryStartOrigin.ContinueMostRecent);
            }
        }
    }
}

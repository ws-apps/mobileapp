﻿using System;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.Shortcuts;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class CreateTimeEntryInteractor : IInteractor<IObservable<IDatabaseTimeEntry>>
    {
        private readonly TimeSpan? duration;
        private readonly IIdProvider idProvider;
        private readonly DateTimeOffset startTime;
        private readonly ITimeService timeService;
        private readonly TimeEntryStartOrigin origin;
        private readonly ITogglDataSource dataSource;
        private readonly ITimeEntryPrototype prototype;
        private readonly IAnalyticsService analyticsService;
        private readonly IApplicationShortcutCreator shortcutCreator;

        public CreateTimeEntryInteractor(
            IIdProvider idProvider, 
            ITimeService timeService, 
            ITogglDataSource dataSource, 
            IAnalyticsService analyticsService, 
            IApplicationShortcutCreator shortcutCreator, 
            ITimeEntryPrototype prototype, 
            DateTimeOffset startTime, 
            TimeSpan? duration)
            : this(idProvider, timeService, dataSource, analyticsService, shortcutCreator, prototype, startTime, duration,
                prototype.Duration.HasValue ? TimeEntryStartOrigin.Manual : TimeEntryStartOrigin.Timer) { }

        public CreateTimeEntryInteractor(
            IIdProvider idProvider,
            ITimeService timeService,
            ITogglDataSource dataSource,
            IAnalyticsService analyticsService,
            IApplicationShortcutCreator shortcutCreator,
            ITimeEntryPrototype prototype,
            DateTimeOffset startTime,
            TimeSpan? duration,
            TimeEntryStartOrigin origin)
        {
            Ensure.Argument.IsNotNull(origin, nameof(origin));
            Ensure.Argument.IsNotNull(prototype, nameof(prototype));
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(shortcutCreator, nameof(shortcutCreator));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.origin = origin;
            this.duration = duration;
            this.prototype = prototype;
            this.startTime = startTime;
            this.idProvider = idProvider;
            this.dataSource = dataSource;
            this.timeService = timeService;
            this.shortcutCreator = shortcutCreator;
            this.analyticsService = analyticsService;
        }

        public IObservable<IDatabaseTimeEntry> Execute()
            => dataSource.User.Current
                .Select(userFromPrototype)
                .SelectMany(dataSource.TimeEntries.Create)
                .Do(shortcutCreator.OnTimeEntryStarted)
                .Do(_ => dataSource.SyncManager.PushSync())
                .Do(_ => analyticsService.TrackStartedTimeEntry(origin));

        private TimeEntry userFromPrototype(IDatabaseUser user)
            => idProvider.GetNextIdentifier()
                .Apply(TimeEntry.Builder.Create)
                .SetUserId(user.Id)
                .SetTagIds(prototype.TagIds)
                .SetTaskId(prototype.TaskId)
                .SetStart(startTime)
                .SetDuration(duration)
                .SetBillable(prototype.IsBillable)
                .SetProjectId(prototype.ProjectId)
                .SetDescription(prototype.Description)
                .SetWorkspaceId(prototype.WorkspaceId)
                .SetAt(timeService.CurrentDateTime)
                .SetSyncStatus(SyncStatus.SyncNeeded)
                .Build();
    }
}

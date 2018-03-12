using System;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.Interactors
{
    [Preserve(AllMembers = true)]
    public sealed class InteractorFactory : IInteractorFactory
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IUserPreferences userPreferences;
        private readonly IAnalyticsService analyticsService;
        private readonly IApplicationShortcutCreator shortcutCreator;

        public InteractorFactory(
            IIdProvider idProvider,
            ITimeService timeService,
            ITogglDataSource dataSource,
            IUserPreferences userPreferences,
            IAnalyticsService analyticsService,
            IApplicationShortcutCreator shortcutCreator)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(shortcutCreator, nameof(shortcutCreator));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.dataSource = dataSource;
            this.idProvider = idProvider;
            this.timeService = timeService;
            this.userPreferences = userPreferences;
            this.shortcutCreator = shortcutCreator;
            this.analyticsService = analyticsService;
        }

        public IInteractor<IObservable<IDatabaseTimeEntry>> CreateTimeEntry(ITimeEntryPrototype prototype)
            => new CreateTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService,
                shortcutCreator,
                prototype,
                prototype.StartTime,
                prototype.Duration);

        public IInteractor<IObservable<IDatabaseTimeEntry>> ContinueTimeEntry(ITimeEntryPrototype prototype)
            => new CreateTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService,
                shortcutCreator,
                prototype,
                timeService.CurrentDateTime,
                null,
                TimeEntryStartOrigin.Continue);

        public IInteractor<IObservable<IDatabaseTimeEntry>> StartSuggestion(Suggestion suggestion)
            => new CreateTimeEntryInteractor(
                idProvider,
                timeService,
                dataSource,
                analyticsService,
                shortcutCreator,
                suggestion,
                timeService.CurrentDateTime,
                null,
                TimeEntryStartOrigin.Suggestion);
    }
}

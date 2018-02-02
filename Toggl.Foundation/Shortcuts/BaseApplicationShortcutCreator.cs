using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Shortcuts
{
    public abstract class BaseApplicationShortcutCreator : IApplicationShortcutCreator
    {
        private readonly ISuggestionProviderContainer suggestionProviderContainer;

        private readonly ApplicationShortcut reportsShortcut = new ApplicationShortcut(
            ApplicationUrls.Reports,
            Resources.Reports,
            ShortcutType.Reports
        );

        private readonly ApplicationShortcut startTimeEntryShortcut = new ApplicationShortcut(
            ApplicationUrls.StartTimeEntry,
            Resources.StartTimeEntry,
            ShortcutType.StartTimeEntry
        );

        private IDisposable timeEntrySuggestionsSubscription;

        public BaseApplicationShortcutCreator(ISuggestionProviderContainer suggestionProviderContainer)
        {
            Ensure.Argument.IsNotNull(suggestionProviderContainer, nameof(suggestionProviderContainer));

            this.suggestionProviderContainer = suggestionProviderContainer;
        }

        public void OnLogin()
        {
            SetShortcuts(new[] { reportsShortcut, startTimeEntryShortcut });
        }

        public void OnLogout() => ClearAllShortCuts();

        public void OnTimeEntryStarted(ITimeEntry timeEntry)
        {
            var mostRecentTimeEntryShortcut = new ApplicationShortcut(
                ApplicationUrls.ContinueTimeEntryEntry(timeEntry.Id),
                Resources.MostRecentEntry,
                timeEntry.Description,
                ShortcutType.TimeEntrySuggestion
            );

            timeEntrySuggestionsSubscription = suggestionProviderContainer
                .Providers
                .First()
                .GetSuggestions()
                .FirstAsync()
                .Select(createApplicationShortcut)
                .Subscribe(suggestionShortcut =>
                {
                    SetShortcuts(new[]
                    {
                        reportsShortcut,
                        startTimeEntryShortcut,
                        mostRecentTimeEntryShortcut,
                        suggestionShortcut
                    });
                },
                () => timeEntrySuggestionsSubscription.Dispose());
        }

        private ApplicationShortcut createApplicationShortcut(Suggestion suggestion)
            => new ApplicationShortcut(
                url: ApplicationUrls.StartTimeEntryWith(
                    suggestion.Description,
                    suggestion.WorkspaceId,
                    suggestion.ProjectId,
                    suggestion.TaskId),
                title: Resources.TimeEntrySuggestion,
                subtitle: $"{suggestion.Description} {suggestion.ProjectName}",
                type: ShortcutType.TimeEntrySuggestion
            );

        protected abstract void ClearAllShortCuts();
        protected abstract void SetShortcuts(IEnumerable<ApplicationShortcut> shortcuts);
    }
}

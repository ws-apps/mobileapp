﻿using System;
using System.Reactive;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Reports;
using Toggl.Foundation.Sync;

namespace Toggl.Foundation.DataSources
{
    public interface ITogglDataSource
    {
        ITagsSource Tags { get; }
        IUserSource User { get; }
        IPreferencesSource Preferences { get; }
        ITasksSource Tasks { get; }
        IClientsSource Clients { get; }
        IProjectsSource Projects { get; }
        IWorkspacesSource Workspaces { get; }
        ITimeEntriesSource TimeEntries { get; }

        ISyncManager SyncManager { get; }
        IAutocompleteProvider AutocompleteProvider { get; }

        IObservable<Unit> StartSyncing();
        IReportsProvider ReportsProvider { get; }

        IObservable<bool> HasUnsyncedData();

        IObservable<Unit> Logout();
    }
}

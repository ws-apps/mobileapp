﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Reports;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;

namespace Toggl.Foundation.DataSources
{
    public sealed class TogglDataSource : ITogglDataSource
    {
        private readonly ITogglDatabase database;
        private readonly IApiErrorHandlingService apiErrorHandlingService;
        private readonly IBackgroundService backgroundService;
        private readonly IApplicationShortcutCreator shortcutCreator;

        private readonly TimeSpan minimumTimeInBackgroundForFullSync;

        private IDisposable errorHandlingDisposable;
        private IDisposable signalDisposable;

        private bool isLoggedIn;

        public TogglDataSource(
            ITogglApi api,
            ITogglDatabase database,
            ITimeService timeService,
            IApiErrorHandlingService apiErrorHandlingService,
            IBackgroundService backgroundService,
            Func<ITogglDataSource, ISyncManager> createSyncManager,
            TimeSpan minimumTimeInBackgroundForFullSync,
            IApplicationShortcutCreator shortcutCreator)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(database, nameof(database));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(apiErrorHandlingService, nameof(apiErrorHandlingService));
            Ensure.Argument.IsNotNull(backgroundService, nameof(backgroundService));
            Ensure.Argument.IsNotNull(createSyncManager, nameof(createSyncManager));
            Ensure.Argument.IsNotNull(shortcutCreator, nameof(shortcutCreator));

            this.database = database;
            this.apiErrorHandlingService = apiErrorHandlingService;
            this.backgroundService = backgroundService;
            this.shortcutCreator = shortcutCreator;

            this.minimumTimeInBackgroundForFullSync = minimumTimeInBackgroundForFullSync;

            User = new UserDataSource(database.User);
            Tags = new TagsDataSource(database.IdProvider, database.Tags, timeService);
            Tasks = new TasksDataSource(database.Tasks);
            Workspaces = new WorkspacesDataSource(database);
            Clients = new ClientsDataSource(database.IdProvider, database.Clients, timeService);
            Preferences = new PreferencesDataSource(database.Preferences);
            Projects = new ProjectsDataSource(database.IdProvider, database.Projects, timeService);
            TimeEntries = new TimeEntriesDataSource(database.IdProvider, database.TimeEntries, timeService);

            AutocompleteProvider = new AutocompleteProvider(database);
            SyncManager = createSyncManager(this);

            ReportsProvider = new ReportsProvider(api, database);

            errorHandlingDisposable = SyncManager.ProgressObservable.Subscribe(onSyncError);
            isLoggedIn = true;
        }

        public IUserSource User { get; }
        public ITagsSource Tags { get; }
        public ITasksSource Tasks { get; }
        public IClientsSource Clients { get; }
        public IPreferencesSource Preferences { get; }
        public IProjectsSource Projects { get; }
        public IWorkspacesSource Workspaces { get; }
        public ITimeEntriesSource TimeEntries { get; }

        public ISyncManager SyncManager { get; }
        public IAutocompleteProvider AutocompleteProvider { get; }

        public IReportsProvider ReportsProvider { get; }

        public IObservable<Unit> StartSyncing()
        {
            if (isLoggedIn == false)
                throw new InvalidOperationException("Cannot start syncing after the user logged out of the app.");

            if (signalDisposable != null)
                throw new InvalidOperationException("The StartSyncing method has already been called.");

            signalDisposable = backgroundService.AppResumedFromBackground
                .Where(timeInBackground => timeInBackground >= minimumTimeInBackgroundForFullSync)
                .Subscribe((TimeSpan _) => SyncManager.ForceFullSync());

            return SyncManager.ForceFullSync()
                .Select(_ => Unit.Default);
        }

        public IObservable<bool> HasUnsyncedData()
            => Observable.Merge(
                hasUnsyncedData(database.TimeEntries),
                hasUnsyncedData(database.Projects),
                hasUnsyncedData(database.User),
                hasUnsyncedData(database.Tasks),
                hasUnsyncedData(database.Clients),
                hasUnsyncedData(database.Tags),
                hasUnsyncedData(database.Workspaces))
                .Any(hasUnsynced => hasUnsynced);

        public IObservable<Unit> Logout()
            => SyncManager.Freeze()
                .FirstAsync()
                .Do(_ => isLoggedIn = false)
                .Do(_ => stopSyncingOnSignal())
                .SelectMany(_ => database.Clear())
                .Do(_ => shortcutCreator.OnLogout())
                .FirstAsync();

        private IObservable<bool> hasUnsyncedData<TModel>(IRepository<TModel> repository)
            where TModel : IDatabaseSyncable
            => repository
                .GetAll(entity => entity.SyncStatus != SyncStatus.InSync)
                .Select(unsynced => unsynced.Any())
                .SingleAsync();

        private void onSyncError(Exception exception)
        {
            if (apiErrorHandlingService.TryHandleDeprecationError(exception)
                || apiErrorHandlingService.TryHandleUnauthorizedError(exception))
            {
                stopSyncingOnSignal();
                return;
            }

            throw new ArgumentException($"{nameof(TogglDataSource)} could not handle unknown sync error {exception.GetType().FullName}.", exception);
        }

        private void stopSyncingOnSignal()
            => signalDisposable?.Dispose();
    }
}

using System;
using Toggl.Foundation.Sync.ConflictResolution.Selectors;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Sync.ConflictResolution
{
    internal static class Resolver
    {
        public static IConflictResolver<IDatabaseClient> ForClients()
            => new PreferNewer<IDatabaseClient>(new ClientSyncSelector());

        public static IConflictResolver<IDatabaseProject> ForProjects()
            => new PreferNewer<IDatabaseProject>(new ProjectSyncSelector());

        public static IConflictResolver<IDatabaseWorkspace> ForWorkspaces()
            => new PreferNewer<IDatabaseWorkspace>(new WorkspaceSyncSelector());

        internal static IConflictResolver<IDatabasePreferences> ForPreferences()
            => new OverwriteUnlessNeedsSync<IDatabasePreferences>();

        public static IConflictResolver<IDatabaseWorkspaceFeatureCollection> ForWorkspaceFeatures()
            => new AlwaysOverwrite<IDatabaseWorkspaceFeatureCollection>();

        public static IConflictResolver<IDatabaseTask> ForTasks()
            => new PreferNewer<IDatabaseTask>(new TaskSyncSelector());

        public static IConflictResolver<IDatabaseTag> ForTags()
            => new PreferNewer<IDatabaseTag>(new TagSyncSelector());

        public static IConflictResolver<IDatabaseTimeEntry> ForTimeEntries()
            => new PreferNewer<IDatabaseTimeEntry>(new TimeEntrySyncSelector(), TimeSpan.FromSeconds(5));
    }
}

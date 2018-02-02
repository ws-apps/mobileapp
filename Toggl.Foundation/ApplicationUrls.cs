namespace Toggl.Foundation
{
    public static class ApplicationUrls
    {
        public static string Reports = "toggl://reports";

        public static string StartTimeEntry = "toggl://start";

        public static string ContinueTimeEntryEntry(long id) => $"toggl://continue?id={id}";

        public static string StartTimeEntryWith(string description, long workspaceId, long? projectId, long? taskId)
            => $"toggl://start?description={description}&workspaceId={workspaceId}&projectId={projectId}&taskId={taskId}";
    }
}

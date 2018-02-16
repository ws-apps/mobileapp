using System;
namespace Toggl.Foundation.Analytics
{
    public interface IAnalyticsService
    {
        void TrackOnboardingSkipEvent(string pageName);

        void TrackLoginEvent();
        void TrackSignUpEvent();

        void TrackCurrentPage(Type viewModelType);

        void TrackNonFatalException(Exception ex);

        void TrackStartedTimeEntry(TimeEntryStartOrigin origin);

        void TrackSyncError(Exception exception);
    }
}

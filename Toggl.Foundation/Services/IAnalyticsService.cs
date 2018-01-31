using System;
namespace Toggl.Foundation.Services
{
    public interface IAnalyticsService
    {
        void TrackOnboardingSkipEvent(int page);

        void TrackLoginEvent();
        void TrackSignUpEvent();

        void TrackCurrentPage(Type viewModelType);

        void TrackNonFatalException(Exception ex);
    }
}

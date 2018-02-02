using System;
using Toggl.Foundation.Services;

namespace Toggl.Giskard.Services
{
    public sealed class AnalyticsService : IAnalyticsService
    {
        public void TrackCurrentPage(Type viewModelType)
        {
        }

        public void TrackLoginEvent()
        {
        }

        public void TrackNonFatalException(Exception ex)
        {
        }

        public void TrackOnboardingSkipEvent(string pageName)
        {
        }

        public void TrackSignUpEvent()
        {
        }
    }
}

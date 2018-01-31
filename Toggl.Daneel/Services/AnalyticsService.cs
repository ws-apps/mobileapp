using System;
using Toggl.Foundation.Services;

namespace Toggl.Daneel.Services
{
    public sealed class AnalyticsService : IAnalyticsService
    {
        public void TrackOnboardingSkipEvent(int page)
        {
        }

        public void TrackCurrentPage(Type viewModelType)
        {
        }

        public void TrackLoginEvent()
        {
        }

        public void TrackNonFatalException(Exception ex)
        {
        }

        public void TrackSignUpEvent()
        {
        }
    }
}

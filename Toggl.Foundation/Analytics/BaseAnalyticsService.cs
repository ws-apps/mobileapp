using System;
using System.Collections.Generic;

namespace Toggl.Foundation.Analytics
{
    public abstract class BaseAnalyticsService : IAnalyticsService
    {
        private const string pageParameter = "PageWhenSkipWasClicked";
        private const string originParameter = "Origin";

        private const string onboardingSkipEventName = "OnboardingSkip";
        private const string startTimeEntryEventName = "TimeEntryStarted";

        public void TrackOnboardingSkipEvent(string pageName)
        {
            var dict = new Dictionary<string, string> { { pageParameter, pageName } };
            NativeTrackEvent(onboardingSkipEventName, dict);
        }

        public void TrackStartedTimeEntry(TimeEntryStartOrigin origin)
        {
            var dict = new Dictionary<string, string> { { originParameter, origin.ToString() } };
            NativeTrackEvent(startTimeEntryEventName, dict);
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

        protected abstract void NativeTrackEvent(string eventName, Dictionary<string, string> parameters);
    }
}

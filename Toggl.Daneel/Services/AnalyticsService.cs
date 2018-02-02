using System;
using Firebase.Analytics;
using Foundation;
using Toggl.Foundation.Services;

namespace Toggl.Daneel.Services
{
    public sealed class AnalyticsService : IAnalyticsService
    {
        private static readonly NSString onboardingSkipEventName = new NSString("OnboardingSkip");
        private static readonly NSString pageParameter = new NSString("PageWhenSkipWasClicked");

        public void TrackOnboardingSkipEvent(string pageName)
        {
            var dict = new NSDictionary<NSString, NSObject>(pageParameter, new NSString(pageName));
            Analytics.LogEvent(onboardingSkipEventName, dict);
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

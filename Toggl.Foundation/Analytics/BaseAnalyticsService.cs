﻿using System;
using System.Collections.Generic;

namespace Toggl.Foundation.Analytics
{
    public abstract class BaseAnalyticsService : IAnalyticsService
    {
        private const string originParameter = "Origin";
        private const string authenticationMethodParameter = "AuthenticationMethod";
        private const string pageParameter = "PageWhenSkipWasClicked";
        private const string viewModelNameParameter = "ViewModelName";

        private const string currentPageEventName = "CurrentPage";
        private const string onboardingSkipEventName = "OnboardingSkip";
        private const string startTimeEntryEventName = "TimeEntryStarted";

        private const string loginEventName = "Login";
        private const string signupEventName = "SignUp";
        private const string resetPasswordEventName = "ResetPassword";

        private const string passwordManagerButtonClicked = "PasswordManagerButtonClicked";
        private const string passwordManagerContainsValidEmail = "PasswordManagerContainsValidEmail";
        private const string passwordManagerContainsValidPassword = "PasswordManagerContainsValidPassword";

        public void TrackOnboardingSkipEvent(string pageName)
        {
            track(onboardingSkipEventName, pageParameter, pageName);
        }

        public void TrackStartedTimeEntry(TimeEntryStartOrigin origin)
        {
            track(startTimeEntryEventName, originParameter, origin.ToString());
        }

        public void TrackCurrentPage(Type viewModelType)
        {
            var dict = new Dictionary<string, string> { { viewModelNameParameter, viewModelType.ToString() } };
            NativeTrackEvent(currentPageEventName, dict);
        }

        public void TrackLoginEvent(AuthenticationMethod authenticationMethod)
        {
            track(loginEventName, authenticationMethodParameter, authenticationMethod.ToString());
        }

        public void TrackSignUpEvent(AuthenticationMethod authenticationMethod)
        {
            track(signupEventName, authenticationMethodParameter, authenticationMethod.ToString());
        }

        public void TrackResetPassword()
        {
            track(resetPasswordEventName);
        }

        public void TrackPasswordManagerButtonClicked()
        {
            track(passwordManagerButtonClicked);
        }

        public void TrackPasswordManagerContainsValidEmail()
        {
            track(passwordManagerContainsValidEmail);
        }

        public void TrackPasswordManagerContainsValidPassword()
        {
            track(passwordManagerContainsValidPassword);
        }

        public void TrackNonFatalException(Exception ex)
        {
        }

        public void TrackSyncError(Exception exception)
        {
            NativeTrackException(exception);
        }

        private void track(string eventName)
        {
            var dict = new Dictionary<string, string>();
            NativeTrackEvent(eventName, dict);
        }

        private void track(string eventName, string parameterName, string parameterValue)
        {
            var dict = new Dictionary<string, string> { { parameterName, parameterValue } };
            NativeTrackEvent(eventName, dict);
        }

        protected abstract void NativeTrackEvent(string eventName, Dictionary<string, string> parameters);

        protected abstract void NativeTrackException(Exception exception);
    }
}

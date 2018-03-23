using System;
using Toggl.PrimeRadiant.Onboarding;

namespace Toggl.PrimeRadiant.Settings
{
    public interface IOnboardingStorage
    {
        void SetIsNewUser(bool isNewUser);
        void SetLastOpened(DateTimeOffset dateString);
        void SetCompletedOnboarding();

        bool IsNewUser();
        string GetLastOpened();
        bool CompletedOnboarding();

        bool WasDismissed(IDismissable dismissable);
        void Dismiss(IDismissable dismissable);

        void Reset();
    }
}

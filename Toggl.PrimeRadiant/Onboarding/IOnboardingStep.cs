using System;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.PrimeRadiant.Onboarding
{
    public interface IOnboardingStep
    {
        IOnboardingStorage OnboardingStorage { get; }

        IObservable<bool> ShouldBeVisible { get; }
    }
}

using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.PrimeRadiant.Extensions
{
    public static class OnboardingStepExtensions
    {
        public static DismissableOnboardingStep ToDismissableStep(this IOnboardingStep step, string key = null)
            => new DismissableOnboardingStep(step, key ?? step.GetType().FullName);
    }
}

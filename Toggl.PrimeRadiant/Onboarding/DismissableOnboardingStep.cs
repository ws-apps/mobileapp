using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.PrimeRadiant.Onboarding
{
    public sealed class DismissableOnboardingStep : IDismissable, IOnboardingStep
    {
        private readonly IOnboardingStep onboardingStep;
        private readonly IOnboardingStorage storage;

        public bool ShouldBeVisible
            => storage.WasDismissed(this) == false && onboardingStep.ShouldBeVisible;

        public string Key { get; }

        public DismissableOnboardingStep(IOnboardingStep onboardingStep, string key, IOnboardingStorage storage)
        {
            Ensure.Argument.IsNotNull(onboardingStep, nameof(onboardingStep));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(key, nameof(key));
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.onboardingStep = onboardingStep;
            this.storage = storage;

            Key = key;
        }

        public void Dismiss()
        {
            storage.Dismiss(this);
        }
    }
}

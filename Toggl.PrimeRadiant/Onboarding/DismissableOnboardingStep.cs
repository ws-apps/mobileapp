using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.PrimeRadiant.Onboarding
{
    public sealed class DismissableOnboardingStep : IDismissable, IOnboardingStep
    {
        private readonly IOnboardingStep onboardingStep;
        private readonly ISubject<bool> shouldBeVisibleSubject;

        private bool wasDismissed;
        private IDisposable shouldBeVisibleSubscription;

        public IOnboardingStorage OnboardingStorage => onboardingStep.OnboardingStorage;

        public IObservable<bool> ShouldBeVisible { get; }

        public string Key { get; }

        public DismissableOnboardingStep(IOnboardingStep onboardingStep, string key)
        {
            Ensure.Argument.IsNotNull(onboardingStep, nameof(onboardingStep));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(key, nameof(key));

            this.onboardingStep = onboardingStep;

            Key = key;

            wasDismissed = OnboardingStorage.WasDismissed(this);
            shouldBeVisibleSubject = new BehaviorSubject<bool>(!wasDismissed);
            shouldBeVisibleSubscription = onboardingStep.ShouldBeVisible.Subscribe(onShouldBeVisibleValue);

            ShouldBeVisible = shouldBeVisibleSubject.AsObservable();
        }

        public void Dismiss()
        {
            wasDismissed = true;
            OnboardingStorage.Dismiss(this);
            shouldBeVisibleSubject.OnNext(false);
        }

        private void onShouldBeVisibleValue(bool shouldBeVisible)
        {
            if (wasDismissed) return;

            shouldBeVisibleSubject.OnNext(shouldBeVisible);
        }
    }
}

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
        private readonly IOnboardingStorage storage;
        private readonly ISubject<bool> shouldBeVisibleSubject;

        private bool wasDismissed;
        private IDisposable shouldBeVisibleSubscription;

        public IObservable<bool> ShouldBeVisible { get; }

        public string Key { get; }

        public DismissableOnboardingStep(IOnboardingStep onboardingStep, string key, IOnboardingStorage storage)
        {
            Ensure.Argument.IsNotNull(onboardingStep, nameof(onboardingStep));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(key, nameof(key));
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.onboardingStep = onboardingStep;
            this.storage = storage;

            Key = key;

            wasDismissed = storage.WasDismissed(this);
            shouldBeVisibleSubject = new BehaviorSubject<bool>(!wasDismissed);
            shouldBeVisibleSubscription = onboardingStep.ShouldBeVisible.Subscribe(onShouldBeVisibleValue);

            ShouldBeVisible = shouldBeVisibleSubject.AsObservable();
        }

        public void Dismiss()
        {
            wasDismissed = true;
            storage.Dismiss(this);
            shouldBeVisibleSubject.OnNext(false);
        }

        private void onShouldBeVisibleValue(bool shouldBeVisible)
        {
            if (wasDismissed) return;

            shouldBeVisibleSubject.OnNext(shouldBeVisible);
        }
    }
}

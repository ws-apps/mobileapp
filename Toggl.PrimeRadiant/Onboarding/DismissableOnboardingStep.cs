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

        private IDisposable internalSubscription;

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

            var wasAlreadyDismissed = storage.WasDismissed(this);
            shouldBeVisibleSubject = new BehaviorSubject<bool>(!wasAlreadyDismissed);
            internalSubscription = onboardingStep.ShouldBeVisible.Subscribe(shouldBeVisibleSubject.OnNext);

            ShouldBeVisible = shouldBeVisibleSubject.AsObservable();
        }

        public void Dismiss()
        {
            storage.Dismiss(this);
            shouldBeVisibleSubject.OnNext(false);
            shouldBeVisibleSubject.OnCompleted();
        }
    }
}

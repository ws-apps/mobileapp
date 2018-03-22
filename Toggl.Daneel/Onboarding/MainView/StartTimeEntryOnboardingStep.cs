﻿using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Daneel.Onboarding.MainView
{
    public sealed class StartTimeEntryOnboardingStep : IOnboardingStep
    {
        public IOnboardingStorage OnboardingStorage { get; }

        public IObservable<bool> ShouldBeVisible { get; }

        public StartTimeEntryOnboardingStep(IOnboardingStorage onboardingStorage)
        {
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));

            OnboardingStorage = onboardingStorage;

            ShouldBeVisible = onboardingStorage.IsNewUser.CombineLatest(onboardingStorage.StartButtonWasTappedBefore,
                (isNewUser, startButtonWasTapped) => isNewUser && !startButtonWasTapped);
        }
    }
}

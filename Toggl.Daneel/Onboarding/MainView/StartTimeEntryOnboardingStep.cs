using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Onboarding;

namespace Toggl.Daneel.Onboarding.MainView
{
    public sealed class StartTimeEntryOnboardingStep : IOnboardingStep
    {
        private readonly ISubject<bool> shouldBeVisibleSubject;
        private readonly MainViewModel mainViewModel;

        private bool shouldBeVisible => mainViewModel.IsWelcome;

        public IObservable<bool> ShouldBeVisible { get; }

        public StartTimeEntryOnboardingStep(MainViewModel mainViewModel)
        {
            Ensure.Argument.IsNotNull(mainViewModel, nameof(mainViewModel));

            this.mainViewModel = mainViewModel;

            shouldBeVisibleSubject = new BehaviorSubject<bool>(shouldBeVisible);
            ShouldBeVisible = shouldBeVisibleSubject.AsObservable().DistinctUntilChanged();

            mainViewModel.PropertyChanged += onPropertyChanged;
        }

        private void onPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            shouldBeVisibleSubject.OnNext(shouldBeVisible);
        }
    }
}

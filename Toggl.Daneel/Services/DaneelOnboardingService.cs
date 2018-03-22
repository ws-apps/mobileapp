using Toggl.Daneel.Onboarding.MainView;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Onboarding;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Daneel.Services
{
    public sealed class DaneelOnboardingService : IOnboardingService
    {
        private readonly IOnboardingStorage storage;

        public DaneelOnboardingService(IOnboardingStorage storage)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.storage = storage;
        }

        public DismissableOnboardingStep CreateStartTimeEntryOnboardingStep(MainViewModel mainViewModel)
            => dismissable(new StartTimeEntryOnboardingStep(mainViewModel), "StartTimeEntryButton");

        private DismissableOnboardingStep dismissable(IOnboardingStep step, string key)
            => new DismissableOnboardingStep(step, key, storage);
    }
}

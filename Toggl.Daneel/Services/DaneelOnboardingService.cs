using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Daneel.Services
{
    public sealed class DaneelOnboardingService : IOnboardingService
    {
        public IOnboardingStorage OnboardingStorage { get; }

        public DaneelOnboardingService(IOnboardingStorage storage)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            OnboardingStorage = storage;
        }
    }
}

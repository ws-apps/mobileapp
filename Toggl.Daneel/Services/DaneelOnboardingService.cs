using System;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Daneel.Services
{
    public sealed class DaneelOnboardingService : IOnboardingService
    {
        private readonly IOnboardingStorage storage;

        public DaneelOnboardingService(IOnboardingStorage storage)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));
        }
    }
}

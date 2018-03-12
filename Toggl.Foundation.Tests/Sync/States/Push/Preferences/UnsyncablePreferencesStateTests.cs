using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class UnsyncablePreferencesStateTests : BaseUnsyncableEntityStateTests
    {
        public UnsyncablePreferencesStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod : TheStartMethod<IDatabasePreferences>
        {
            protected override BaseUnsyncableEntityState<IDatabasePreferences> CreateState(IRepository<IDatabasePreferences> repository)
                => new UnsyncablePreferencesState(repository);

            protected override IDatabasePreferences CreateDirtyEntity()
                => Preferences.Dirty(new Ultrawave.Models.Preferences());
        }
    }
}

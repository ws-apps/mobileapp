using System;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class UpdatePreferencesStateTests : BaseUpdateEntityStateTests
    {
        public UpdatePreferencesStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod : TheStartMethod<IDatabasePreferences, IPreferences>
        {
            protected override BasePushEntityState<IDatabasePreferences> CreateState(ITogglApi api, IRepository<IDatabasePreferences> repository)
                => new UpdatePreferencesState(api, repository);

            protected override Func<IDatabasePreferences, IObservable<IPreferences>> GetUpdateFunction(ITogglApi api)
                => preferences => api.Preferences.Update(preferences).Select(_ => preferences);

            protected override IDatabasePreferences CreateDirtyEntity(long id, DateTimeOffset lastUpdate = default(DateTimeOffset))
                => Preferences.Dirty(new Ultrawave.Models.Preferences());

            protected override void AssertUpdateReceived(ITogglApi api, IDatabasePreferences entity)
                => api.Preferences.Received().Update(entity);

            protected override IDatabasePreferences CreateDirtyEntityWithNegativeId()
                => Preferences.Dirty(new Ultrawave.Models.Preferences());

            protected override IDatabasePreferences CreateCleanWithPositiveIdFrom(IDatabasePreferences entity)
            {
                var preferences = new Ultrawave.Models.Preferences(entity);
                return Preferences.Clean(preferences);
            }

            protected override IDatabasePreferences CreateCleanEntityFrom(IDatabasePreferences entity)
                => Preferences.Clean(entity);
        }
    }
}

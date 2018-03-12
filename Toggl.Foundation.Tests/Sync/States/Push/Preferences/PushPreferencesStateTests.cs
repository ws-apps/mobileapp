using System;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public class PushPreferencesStateTests : BasePushStateTests
    {
        public PushPreferencesStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabasePreferences>
        {
            protected override BasePushState<IDatabasePreferences> CreateState(IRepository<IDatabasePreferences> repository)
                => new PushPreferencesState(repository);

            protected override IDatabasePreferences CreateUnsyncedEntity(DateTimeOffset lastUpdate = default(DateTimeOffset))
                => Preferences.Dirty(new Ultrawave.Models.Preferences());

            protected override void SetupRepositoryToReturn(IRepository<IDatabasePreferences> repository, IDatabasePreferences[] entities)
            {
                repository.GetAll(Arg.Any<Func<IDatabasePreferences, bool>>()).Returns(Observable.Return(entities));
            }

            protected override void SetupRepositoryToThrow(IRepository<IDatabasePreferences> repository)
            {
                repository.GetAll(Arg.Any<Func<IDatabasePreferences, bool>>()).Returns(_ => throw new TestException());
            }
        }
    }
}

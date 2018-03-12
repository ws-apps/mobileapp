using System;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public class PushClientsStateTests : BasePushStateTests
    {
        public PushClientsStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseClient>
        {
            protected override BasePushState<IDatabaseClient> CreateState(IRepository<IDatabaseClient> repository)
                => new PushClientsState(repository);

            protected override IDatabaseClient CreateUnsyncedEntity(DateTimeOffset lastUpdate = default(DateTimeOffset))
                => Client.Dirty(new MockClient { At = lastUpdate });

            protected override void SetupRepositoryToReturn(IRepository<IDatabaseClient> repository, IDatabaseClient[] entities)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseClient, bool>>()).Returns(Observable.Return(entities));
            }

            protected override void SetupRepositoryToThrow(IRepository<IDatabaseClient> repository)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseClient, bool>>()).Returns(_ => { throw new TestException(); });
            }
        }
    }
}

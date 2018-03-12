using System;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class UnsyncableClientStateTests : BaseUnsyncableEntityStateTests
    {
        public UnsyncableClientStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod : TheStartMethod<IDatabaseClient>
        {
            protected override BaseUnsyncableEntityState<IDatabaseClient> CreateState(IRepository<IDatabaseClient> repository)
                => new UnsyncableClientState(repository);

            protected override IDatabaseClient CreateDirtyEntity()
                => Client.Dirty(new MockClient { Name = Guid.NewGuid().ToString() });
        }
    }
}

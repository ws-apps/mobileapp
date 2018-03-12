using System;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class CreateClientTests : BaseCreateEntityStateTests
    {
        public CreateClientTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseClient, IClient>
        {
            protected override IDatabaseClient CreateDirtyEntityWithNegativeId()
                => Client.Dirty(new MockClient { Id = -123, Name = Guid.NewGuid().ToString() });

            protected override IDatabaseClient CreateCleanWithPositiveIdFrom(IDatabaseClient entity)
                => Client.Clean(new MockClient { Id = 456, Name = entity.Name });

            protected override IDatabaseClient CreateCleanEntityFrom(IDatabaseClient entity)
                => Client.Clean(entity);

            protected override BasePushEntityState<IDatabaseClient> CreateState(ITogglApi api, IRepository<IDatabaseClient> repository)
                => new CreateClientState(api, repository);

            protected override Func<IDatabaseClient, IObservable<IClient>> GetCreateFunction(ITogglApi api)
                => api.Clients.Create;

            protected override bool EntitiesHaveSameImportantProperties(IDatabaseClient a, IDatabaseClient b)
                => a.Name == b.Name;
        }
    }
}

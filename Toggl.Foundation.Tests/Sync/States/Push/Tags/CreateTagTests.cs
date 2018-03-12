using System;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class CreateTagTests : BaseCreateEntityStateTests
    {
        public CreateTagTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseTag, ITag>
        {
            protected override IDatabaseTag CreateDirtyEntityWithNegativeId()
                => Tag.Dirty(new MockTag { Id = -123, Name = Guid.NewGuid().ToString() });

            protected override IDatabaseTag CreateCleanWithPositiveIdFrom(IDatabaseTag entity)
                => Tag.Clean(new MockTag { Id = 456, Name = entity.Name });

            protected override IDatabaseTag CreateCleanEntityFrom(IDatabaseTag entity)
                => Tag.Clean(entity);

            protected override BasePushEntityState<IDatabaseTag> CreateState(ITogglApi api, IRepository<IDatabaseTag> repository)
                => new CreateTagState(api, repository);

            protected override Func<IDatabaseTag, IObservable<ITag>> GetCreateFunction(ITogglApi api)
                => api.Tags.Create;

            protected override bool EntitiesHaveSameImportantProperties(IDatabaseTag a, IDatabaseTag b)
                => a.Name == b.Name;
        }
    }
}

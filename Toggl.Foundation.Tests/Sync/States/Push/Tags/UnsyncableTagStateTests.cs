using System;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class UnsyncableTagStateTests : BaseUnsyncableEntityStateTests
    {
        public UnsyncableTagStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod : TheStartMethod<IDatabaseTag>
        {
            protected override BaseUnsyncableEntityState<IDatabaseTag> CreateState(IRepository<IDatabaseTag> repository)
                => new UnsyncableTagState(repository);

            protected override IDatabaseTag CreateDirtyEntity()
                => Tag.Dirty(new MockTag { Name = Guid.NewGuid().ToString() });
        }
    }
}

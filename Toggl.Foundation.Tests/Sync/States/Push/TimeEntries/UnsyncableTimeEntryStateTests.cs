using System;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public sealed class UnsyncableTimeEntryStateTests : BaseUnsyncableEntityStateTests
    {
        public UnsyncableTimeEntryStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod : TheStartMethod<IDatabaseTimeEntry>
        {
            protected override BaseUnsyncableEntityState<IDatabaseTimeEntry> CreateState(IRepository<IDatabaseTimeEntry> repository)
                => new UnsyncableTimeEntryState(repository);

            protected override IDatabaseTimeEntry CreateDirtyEntity()
                => TimeEntry.Dirty(new MockTimeEntry { Description = Guid.NewGuid().ToString() });
        }
    }
}

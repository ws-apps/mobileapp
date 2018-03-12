using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class DeleteTimeEntryStateTests : BaseDeleteEntityStateTests
    {
        public DeleteTimeEntryStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseTimeEntry, ITimeEntry>
        {
            protected override IDatabaseTimeEntry CreateDirtyEntityWithNegativeId()
                => TimeEntry.Dirty(new MockTimeEntry { Id = -123, Description = Guid.NewGuid().ToString() });

            protected override IDatabaseTimeEntry CreateCleanWithPositiveIdFrom(IDatabaseTimeEntry entity)
                => TimeEntry.Clean(new MockTimeEntry { Id = 456, Description = entity.Description });

            protected override IDatabaseTimeEntry CreateCleanEntityFrom(IDatabaseTimeEntry entity)
                => TimeEntry.Clean(entity);

            protected override BasePushEntityState<IDatabaseTimeEntry> CreateState(ITogglApi api, IRepository<IDatabaseTimeEntry> repository)
                => new DeleteTimeEntryState(api, repository);

            protected override Func<IDatabaseTimeEntry, IObservable<Unit>> GetDeleteFunction(ITogglApi api)
                => timeEntry => api.TimeEntries.Delete(timeEntry);
        }
    }
}

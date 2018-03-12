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
    public sealed class UpdateTimeEntryStateTests : BaseUpdateEntityStateTests
    {
        public UpdateTimeEntryStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod : TheStartMethod<IDatabaseTimeEntry, ITimeEntry>
        {
            protected override BasePushEntityState<IDatabaseTimeEntry> CreateState(ITogglApi api, IRepository<IDatabaseTimeEntry> repository)
                => new UpdateTimeEntryState(api, repository);

            protected override Func<IDatabaseTimeEntry, IObservable<ITimeEntry>> GetUpdateFunction(ITogglApi api)
                => api.TimeEntries.Update;

            protected override IDatabaseTimeEntry CreateDirtyEntity(long id, DateTimeOffset lastUpdate = default(DateTimeOffset))
                => TimeEntry.Dirty(new MockTimeEntry { Id = id, Description = Guid.NewGuid().ToString(), At = lastUpdate });

            protected override void AssertUpdateReceived(ITogglApi api, IDatabaseTimeEntry entity)
                => api.TimeEntries.Received().Update(entity);

            protected override IDatabaseTimeEntry CreateDirtyEntityWithNegativeId()
                => TimeEntry.Dirty(new MockTimeEntry { Id = -1, Description = Guid.NewGuid().ToString() });

            protected override IDatabaseTimeEntry CreateCleanWithPositiveIdFrom(IDatabaseTimeEntry entity)
            {
                var te = new MockTimeEntry(entity);
                te.Id = 1;
                return TimeEntry.Clean(te);
            }

            protected override IDatabaseTimeEntry CreateCleanEntityFrom(IDatabaseTimeEntry entity)
                => TimeEntry.Clean(entity);
        }
    }
}

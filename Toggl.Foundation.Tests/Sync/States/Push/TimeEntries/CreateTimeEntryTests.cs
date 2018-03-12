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
    public sealed class CreateTimeEntryStateTests : BaseCreateEntityStateTests
    {
        public CreateTimeEntryStateTests()
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
                => new CreateTimeEntryState(api, repository);

            protected override Func<IDatabaseTimeEntry, IObservable<ITimeEntry>> GetCreateFunction(ITogglApi api)
                => api.TimeEntries.Create;

            protected override bool EntitiesHaveSameImportantProperties(IDatabaseTimeEntry a, IDatabaseTimeEntry b)
                => a.Description == b.Description;
        }
    }
}

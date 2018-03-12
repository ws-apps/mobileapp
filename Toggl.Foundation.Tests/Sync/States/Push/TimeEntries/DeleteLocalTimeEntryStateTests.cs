using System;
using System.Reactive;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class DeleteLocalTimeEntryStateTests : BaseDeleteLocalEntityStateTests<IDatabaseTimeEntry>
    {
        protected override IDatabaseTimeEntry CreateEntity()
            => TimeEntry.Dirty(new MockTimeEntry { Id = 123 });

        protected override BaseDeleteLocalEntityState<IDatabaseTimeEntry> CreateState(IRepository<IDatabaseTimeEntry> repository)
            => new DeleteLocalTimeEntryState(repository);

        protected override void PrepareDatabaseOperationToThrow(IRepository<IDatabaseTimeEntry> repository, Exception e)
            => repository.Delete(Arg.Any<long>()).Returns(Observable.Throw<Unit>(e));
    }
}

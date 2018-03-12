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
    public class PushTimeEntriesStateTests : BasePushStateTests
    {
        public PushTimeEntriesStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseTimeEntry>
        {
            protected override BasePushState<IDatabaseTimeEntry> CreateState(IRepository<IDatabaseTimeEntry> repository)
                => new PushTimeEntriesState(repository);

            protected override IDatabaseTimeEntry CreateUnsyncedEntity(DateTimeOffset lastUpdate = default(DateTimeOffset))
                => TimeEntry.Dirty(new MockTimeEntry { At = lastUpdate });

            protected override void SetupRepositoryToReturn(IRepository<IDatabaseTimeEntry> repository, IDatabaseTimeEntry[] entities)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>()).Returns(Observable.Return(entities));
            }

            protected override void SetupRepositoryToThrow(IRepository<IDatabaseTimeEntry> repository)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>()).Returns(_ => throw new TestException());
            }
        }
    }
}

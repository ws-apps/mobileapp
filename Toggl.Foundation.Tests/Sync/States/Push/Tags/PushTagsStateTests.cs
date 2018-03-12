using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States.Push
{
    public class PushTagsStateTests : BasePushStateTests
    {
        public PushTagsStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseTag>
        {
            protected override BasePushState<IDatabaseTag> CreateState(IRepository<IDatabaseTag> repository)
                => new PushTagsState(repository);

            protected override IDatabaseTag CreateUnsyncedEntity(DateTimeOffset lastUpdate = default(DateTimeOffset))
                => Tag.Dirty(new MockTag { At = lastUpdate });

            protected override void SetupRepositoryToReturn(IRepository<IDatabaseTag> repository, IDatabaseTag[] entities)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseTag, bool>>()).Returns(Observable.Return(entities));
            }

            protected override void SetupRepositoryToThrow(IRepository<IDatabaseTag> repository)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseTag, bool>>()).Returns(_ => { throw new TestException(); });
            }
        }
    }
}

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
    public class PushProjectStateTests : BasePushStateTests
    {
        public PushProjectStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseProject>
        {
            protected override BasePushState<IDatabaseProject> CreateState(IRepository<IDatabaseProject> repository)
                => new PushProjectsState(repository);

            protected override IDatabaseProject CreateUnsyncedEntity(DateTimeOffset lastUpdate = default(DateTimeOffset))
                => Project.Dirty(new MockProject { At = lastUpdate });

            protected override void SetupRepositoryToReturn(IRepository<IDatabaseProject> repository, IDatabaseProject[] entities)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseProject, bool>>()).Returns(Observable.Return(entities));
            }

            protected override void SetupRepositoryToThrow(IRepository<IDatabaseProject> repository)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseProject, bool>>()).Returns(_ => { throw new TestException(); });
            }
        }
    }
}

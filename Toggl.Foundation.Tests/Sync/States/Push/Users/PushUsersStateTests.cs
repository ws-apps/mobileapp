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
    public class PushUsersStateTests : BasePushStateTests
    {
        public PushUsersStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseUser>
        {
            protected override BasePushState<IDatabaseUser> CreateState(IRepository<IDatabaseUser> repository)
                => new PushUsersState(repository);

            protected override IDatabaseUser CreateUnsyncedEntity(DateTimeOffset lastUpdate = default(DateTimeOffset))
                => User.Dirty(new MockUser { At = lastUpdate });

            protected override void SetupRepositoryToReturn(IRepository<IDatabaseUser> repository, IDatabaseUser[] entities)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseUser, bool>>()).Returns(Observable.Return(entities));
            }

            protected override void SetupRepositoryToThrow(IRepository<IDatabaseUser> repository)
            {
                repository.GetAll(Arg.Any<Func<IDatabaseUser, bool>>()).Returns(_ => throw new TestException());
            }
        }
    }
}

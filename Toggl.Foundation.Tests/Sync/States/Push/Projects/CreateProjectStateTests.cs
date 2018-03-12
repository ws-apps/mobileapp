using System;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class CreateProjectStateTests : BaseCreateEntityStateTests
    {
        public CreateProjectStateTests()
            : base(new TheStartMethod())
        {
        }

        private class TheStartMethod : TheStartMethod<IDatabaseProject, IProject>
        {
            protected override IDatabaseProject CreateDirtyEntityWithNegativeId()
                => Project.Dirty(new MockProject { Id = -123, Name = Guid.NewGuid().ToString() });

            protected override IDatabaseProject CreateCleanWithPositiveIdFrom(IDatabaseProject entity)
                => Project.Clean(new MockProject { Id = 456, Name = entity.Name });

            protected override IDatabaseProject CreateCleanEntityFrom(IDatabaseProject entity)
                => Project.Clean(entity);

            protected override BasePushEntityState<IDatabaseProject> CreateState(ITogglApi api, IRepository<IDatabaseProject> repository)
                => new CreateProjectState(api, repository);

            protected override Func<IDatabaseProject, IObservable<IProject>> GetCreateFunction(ITogglApi api)
                => api.Projects.Create;

            protected override bool EntitiesHaveSameImportantProperties(IDatabaseProject a, IDatabaseProject b)
                => a.Name == b.Name;
        }
    }
}

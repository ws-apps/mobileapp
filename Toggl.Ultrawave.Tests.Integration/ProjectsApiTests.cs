﻿﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac.Models;
 using Toggl.Ultrawave.Exceptions;
 using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
 using Toggl.Ultrawave.Tests.Integration.Helper;
 using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Ultrawave.Tests.Integration
{
    public sealed class ProjectsApiTests
    {
        public sealed class TheGetAllMethod : AuthenticatedEndpointBaseTests<List<IProject>>
        {
            protected override IObservable<List<IProject>> CallEndpointWith(ITogglApi togglApi)
                => togglApi.Projects.GetAll();

            [Fact, LogTestInfo]
            public async System.Threading.Tasks.Task ReturnsAllProjects()
            {
                var (togglClient, user) = await SetupTestUser();

                var projectA = await createNewProject(togglClient, user.DefaultWorkspaceId, createClient: true);
                var projectAPosted = await togglClient.Projects.Create(projectA);

                var projectB = await createNewProject(togglClient, user.DefaultWorkspaceId);
                var projectBPosted = await togglClient.Projects.Create(projectB);

                var projects = await CallEndpointWith(togglClient);

                projects.Should().HaveCount(2);

                projects.Should().Contain(project => isTheSameAs(projectAPosted, project));
                projects.Should().Contain(project => isTheSameAs(projectBPosted, project));
            }

            [Fact, LogTestInfo]
            public async System.Threading.Tasks.Task ReturnsOnlyActiveProjects()
            {
                var (togglClient, user) = await SetupTestUser();

                var activeProject = await createNewProject(togglClient, user.DefaultWorkspaceId);
                var activeProjectPosted = await togglClient.Projects.Create(activeProject);

                var inactiveProject = await createNewProject(togglClient, user.DefaultWorkspaceId, isActive: false);
                var inactiveProjectPosted = await togglClient.Projects.Create(inactiveProject);

                var projects = await CallEndpointWith(togglClient);

                projects.Should().HaveCount(1);
                projects.Should().Contain(project => isTheSameAs(project, activeProjectPosted));
                projects.Should().NotContain(project => isTheSameAs(project, inactiveProjectPosted));
            }

            [Fact, LogTestInfo]
            public async System.Threading.Tasks.Task ReturnsEmptyListWhenThereAreNoActiveProjects()
            {
                var (togglClient, user) = await SetupTestUser();

                var noProjects = await CallEndpointWith(togglClient);

                Project project = await createNewProject(togglClient, user.DefaultWorkspaceId, isActive: false);
                await togglClient.Projects.Create(project);

                project = await createNewProject(togglClient, user.DefaultWorkspaceId, isActive: false);
                await togglClient.Projects.Create(project);

                var activeProjects = await CallEndpointWith(togglClient);

                noProjects.Should().HaveCount(0);
                activeProjects.Should().HaveCount(0);
            }

            public sealed class TheGetAllSinceMethod : AuthenticatedGetSinceEndpointBaseTests<IProject>
            {
                protected override IObservable<List<IProject>> CallEndpointWith(ITogglApi togglApi, DateTimeOffset threshold)
                    => togglApi.Projects.GetAllSince(threshold);

                protected override DateTimeOffset AtDateOf(IProject model)
                    => model.At;

                protected override IProject MakeUniqueModel(ITogglApi api, IUser user)
                    => new Project { Active = true, Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId };

                protected override IObservable<IProject> PostModelToApi(ITogglApi api, IProject model)
                    => api.Projects.Create(model);

                protected override Expression<Func<IProject, bool>> ModelWithSameAttributesAs(IProject model)
                    => p => isTheSameAs(model, p);
            }

            public sealed class TheCreateMethod : AuthenticatedPostEndpointBaseTests<IProject>
            {
                protected override IObservable<IProject> CallEndpointWith(ITogglApi togglApi)
                    => Observable.Defer(async () =>
                    {
                        var user = await togglApi.User.Get();
                        var project = await createNewProject(togglApi, user.DefaultWorkspaceId);
                        return CallEndpointWith(togglApi, project);
                    });

                private IObservable<IProject> CallEndpointWith(ITogglApi togglApi, IProject project)
                    => togglApi.Projects.Create(project);

                [Fact, LogTestInfo]
                public async System.Threading.Tasks.Task CreatesNewProject()
                {
                    var (togglClient, user) = await SetupTestUser();

                    var project = await createNewProject(togglClient, user.DefaultWorkspaceId);
                    var persistedProject = await CallEndpointWith(togglClient, project);

                    persistedProject.Name.Should().Be(project.Name);
                    persistedProject.ClientId.Should().Be(project.ClientId);
                    persistedProject.IsPrivate.Should().Be(project.IsPrivate);
                    persistedProject.Color.Should().Be(project.Color);
                }
            }

            private static async Task<Project> createNewProject(ITogglApi togglClient, long workspaceID, bool isActive = true, bool createClient = false)
            {
                IClient client = null;

                if (createClient)
                {
                    client = new Client
                    {
                        Name = Guid.NewGuid().ToString(),
                        WorkspaceId = workspaceID
                    };

                    client = await togglClient.Clients.Create(client);
                }

                return new Project
                {
                    Name = Guid.NewGuid().ToString(),
                    WorkspaceId = workspaceID,
                    At = DateTimeOffset.UtcNow,
                    Color = "#06aaf5",
                    Active = isActive,
                    ClientId = client?.Id
                };
            }

            private static bool isTheSameAs(IProject a, IProject b)
                => a.Id == b.Id
                && a.Name == b.Name
                && a.ClientId == b.ClientId
                && a.IsPrivate == b.IsPrivate
                && a.Color == b.Color;
        }

        public sealed class TheSearchMethod : AuthenticatedEndpointBaseTests<List<IProject>>
        {
            [Fact, LogTestInfo]
            public async ThreadingTask ThrowsArgumentNullExceptionForNullIds()
            {
                var (togglApi, user) = await SetupTestUser();

                Action searchingNull = () => togglApi.Projects.Search(user.DefaultWorkspaceId, null).Wait();

                searchingNull.ShouldThrow<ArgumentNullException>();
            }

            [Fact, LogTestInfo]
            public async ThreadingTask ThrowsBadRequestExceptionForEmtpyArrayOfIds()
            {
                var (togglApi, user) = await SetupTestUser();
                var projectIds = new long[0];

                Action searchingWithEmptyIds = () => togglApi.Projects.Search(user.DefaultWorkspaceId, projectIds).Wait();

                searchingWithEmptyIds.ShouldThrow<BadRequestException>();
            }

            [Fact, LogTestInfo]
            public async ThreadingTask ReturnsEmtpyArrayForProjectsWhichDontExistOrDoNotBelongToOtherUser()
            {
                var (togglApi, user) = await SetupTestUser();
                var projectIds = new long[] { 1, 2, 3 };

                var projects = await togglApi.Projects.Search(user.DefaultWorkspaceId, projectIds);

                projects.Should().HaveCount(0);
            }

            [Fact, LogTestInfo]
            public async ThreadingTask DoesNotFindProjectInAnInaccessibleWorkspace()
            {
                var (togglApiA, userA) = await SetupTestUser();
                var (togglApiB, userB) = await SetupTestUser();
                var projectA = await togglApiA.Projects.Create(new Project { Name = Guid.NewGuid().ToString(), WorkspaceId = userA.DefaultWorkspaceId });

                var projects = await togglApiB.Projects.Search(userB.DefaultWorkspaceId, new[] { projectA.Id });

                projects.Should().HaveCount(0);
            }

            [Fact, LogTestInfo]
            public async ThreadingTask DoesNotFindProjectInADifferentWorkspace()
            {
                var (togglApi, user) = await SetupTestUser();
                var secondWorkspace = await WorkspaceHelper.CreateFor(user);
                var projectA = await togglApi.Projects.Create(new Project { Name = Guid.NewGuid().ToString(), WorkspaceId = secondWorkspace.Id });
                var projectB = await togglApi.Projects.Create(new Project { Name = Guid.NewGuid().ToString(), WorkspaceId = secondWorkspace.Id });

                var projects = await togglApi.Projects.Search(user.DefaultWorkspaceId, new[] { projectA.Id, projectB.Id });

                projects.Should().HaveCount(0);
            }

            [Fact, LogTestInfo]
            public async ThreadingTask ReturnsOnlyProjectInTheSearchedWorkspace()
            {
                var (togglApi, user) = await SetupTestUser();
                var secondWorkspace = await WorkspaceHelper.CreateFor(user);
                var projectA = await togglApi.Projects.Create(new Project { Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId });
                var projectB = await togglApi.Projects.Create(new Project { Name = Guid.NewGuid().ToString(), WorkspaceId = secondWorkspace.Id });

                var projects = await togglApi.Projects.Search(user.DefaultWorkspaceId, new[] { projectA.Id, projectB.Id });

                projects.Should().HaveCount(1);
                projects.Should().Contain(p => p.Id == projectA.Id);
            }

            protected override IObservable<List<IProject>> CallEndpointWith(ITogglApi togglApi)
                => togglApi.User.Get()
                    .SelectMany(user => togglApi.Projects.Search(user.DefaultWorkspaceId, new[] { -1L }));
        }
    }
}

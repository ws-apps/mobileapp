﻿using System;
using System.Diagnostics;
using System.Reactive.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.DataSources
{
    public sealed class UserDataSourceTests
    {
        public abstract class UserDataSourceTest : BaseDataSourceTests<UserDataSource>
        {
            protected const long userId = 1;
            protected const long InitialWorkspaceId = 1;
            protected const long UpdatedWorkspaceId = 2;

            protected override UserDataSource CreateDataSource()
            {
                IDatabaseUser initialUser = newUser(userId, InitialWorkspaceId);
                IDatabaseUser updatedUser = newUser(userId, UpdatedWorkspaceId);

                Storage = Substitute.For<ISingleObjectStorage<IDatabaseUser>>();

                Storage.Single().Returns(
                    Observable.Return(initialUser),
                    Observable.Return(updatedUser)
                );

                DataBase.User.Returns(Storage);

                return new UserDataSource(DataBase.User, TimeService);
            }

            private IDatabaseUser newUser(long id, long workspaceId)
            {
                IDatabaseUser user = Substitute.For<IDatabaseUser>();
                user.Id.Returns(id);
                user.DefaultWorkspaceId.Returns(workspaceId);
                return user;
            }

            protected ISingleObjectStorage<IDatabaseUser> Storage;
        }

        public sealed class TheCurrentMethod : UserDataSourceTest
        {
            [Fact, LogIfTooSlow]
            public void AccessesUnderlyingStorage()
            {
                DataSource.Current.Wait();

                Storage.Received().Single();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotUseCachedVersion()
            {
                var userA = DataSource.Current.Wait();
                var userB = DataSource.Current.Wait();

                userA.DefaultWorkspaceId.Should().Be(InitialWorkspaceId);
                userB.DefaultWorkspaceId.Should().Be(UpdatedWorkspaceId);
            }
        }

        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyArgumentIsNull(bool useRepository, bool useTimeServide)
            {
                var repository = useRepository ? Substitute.For<ISingleObjectStorage<IDatabaseUser>>() : null;
                var timeService = useTimeServide ? Substitute.For<ITimeService>() : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new UserDataSource(repository, timeService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }
    }
}

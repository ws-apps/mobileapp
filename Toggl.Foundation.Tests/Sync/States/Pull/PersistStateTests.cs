﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public abstract class PersistStateTests
    {
        private readonly ITheStartMethodHelper testHelper;

        protected PersistStateTests(ITheStartMethodHelper helper)
        {
            testHelper = helper;
        }

        [Fact, LogIfTooSlow]
        public void EmitsTransitionToPersistFinished()
            => testHelper.EmitsTransitionToPersistFinished();

        [Fact, LogIfTooSlow]
        public void ThrowsIfFetchObservablePublishesTwice()
            => testHelper.ThrowsIfFetchObservablePublishesTwice();

        [Fact, LogIfTooSlow]
        public void TriggersBatchUpdate()
            => testHelper.TriggersBatchUpdate();

        [Fact, LogIfTooSlow]
        public void DoesNotUpdateSinceParametersWhenNothingIsFetched()
            => testHelper.DoesNotUpdateSinceParametersWhenNothingIsFetched();

        [Fact, LogIfTooSlow]
        public void UpdatesSinceParametersOfTheFetchedEntity()
            => testHelper.UpdatesSinceParametersOfTheFetchedEntity();

        [Fact, LogIfTooSlow]
        public void HandlesNullValueReceivedFromTheServerAsAnEmptyList()
            => testHelper.HandlesNullValueReceivedFromTheServerAsAnEmptyList();

        [Fact, LogIfTooSlow]
        public void SelectsTheLatestAtValue()
            => testHelper.SelectsTheLatestAtValue();

        [Fact, LogIfTooSlow]
        public void PassesTheNewSinceParametersThroughTheTransition()
            => testHelper.PassesTheNewSinceParametersThroughTheTransition();

        [Fact, LogIfTooSlow]
        public void SinceDatesAreNotUpdatedWhenBatchUpdateThrows()
            => testHelper.SinceDatesAreNotUpdatedWhenBatchUpdateThrows();

        [Fact, LogIfTooSlow]
        public void ThrowsWhenBatchUpdateThrows()
            => testHelper.ThrowsWhenBatchUpdateThrows();

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
            => testHelper.ReturnsFailedTransitionWhenHttpFailsWithClientErrorException(exception);

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException reason)
            => testHelper.ReturnsFailedTransitionWhenHttpFailsWithServerErrorException(reason);


        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        public void ThrowsWhenCertainExceptionsAreCaught(Exception exception)
            => testHelper.ThrowsWhenCertainExceptionsAreCaught(exception);

        public interface ITheStartMethodHelper
        {
            void EmitsTransitionToPersistFinished();

            void ThrowsIfFetchObservablePublishesTwice();

            void TriggersBatchUpdate();

            void ThrowsWhenBatchUpdateThrows();

            void DoesNotUpdateSinceParametersWhenNothingIsFetched();

            void UpdatesSinceParametersOfTheFetchedEntity();

            void HandlesNullValueReceivedFromTheServerAsAnEmptyList();

            void SelectsTheLatestAtValue();

            void SinceDatesAreNotUpdatedWhenBatchUpdateThrows();

            void PassesTheNewSinceParametersThroughTheTransition();

            void ReturnsFailedTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception);

            void ReturnsFailedTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception);

            void ThrowsWhenCertainExceptionsAreCaught(Exception exception);
        }

        internal abstract class TheStartMethod<TState, TInterface, TDatabaseInterface> : ITheStartMethodHelper
            where TDatabaseInterface : class, TInterface
            where TState : BasePersistState<TInterface, TDatabaseInterface>
        {
            private readonly IRepository<TDatabaseInterface> repository;
            private readonly ISinceParameterRepository sinceParameterRepository;
            private readonly TState state;

            protected DateTimeOffset Now { get; } = new DateTimeOffset(2017, 04, 05, 12, 34, 56, TimeSpan.Zero);
            protected DateTimeOffset at = new DateTimeOffset(2017, 09, 01, 12, 34, 56, TimeSpan.Zero);

            protected TheStartMethod()
            {
                repository = Substitute.For<IRepository<TDatabaseInterface>>();
                sinceParameterRepository = Substitute.For<ISinceParameterRepository>();
                state = CreateState(repository, sinceParameterRepository);
            }

            public void EmitsTransitionToPersistFinished()
            {
                var observables = CreateObservables();

                var transition = state.Start(observables).SingleAsync().Wait();

                transition.Result.Should().Be(state.FinishedPersisting);
            }

            public void ThrowsIfFetchObservablePublishesTwice()
            {
                var fetchObservables = CreateObservablesWhichFetchesTwice();

                Action fetchTwice = () => state.Start(fetchObservables).Wait();

                fetchTwice.ShouldThrow<InvalidOperationException>();
            }

            public void TriggersBatchUpdate()
            {
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(null, entities);

                state.Start(observables).SingleAsync().Wait();

                assertBatchUpdateWasCalled(entities);
            }

            public void ThrowsWhenBatchUpdateThrows()
            {
                var observables = CreateObservables();
                setupBatchUpdateToThrow(new TestException());

                Action startingState = () => state.Start(observables).SingleAsync().Wait();

                startingState.ShouldThrow<TestException>();
            }

            public void DoesNotUpdateSinceParametersWhenNothingIsFetched()
            {
                var oldSinceParameters = new SinceParameters(
                    null,
                    workspaces: at,
                    clients: at.AddDays(1),
                    projects: at.AddDays(2),
                    tags: at.AddDays(3),
                    tasks: at.AddDays(4),
                    timeEntries: at.AddDays(5)
                );
                var observables = CreateObservables(oldSinceParameters);

                state.Start(observables).SingleAsync().Wait();

                sinceParameterRepository.Received().Set(Arg.Is<ISinceParameters>(
                    newSinceParameters =>
                        newSinceParameters.Workspaces == oldSinceParameters.Workspaces
                        && newSinceParameters.Projects == oldSinceParameters.Projects
                        && newSinceParameters.Clients == oldSinceParameters.Clients
                        && newSinceParameters.Tags == oldSinceParameters.Tags
                        && newSinceParameters.Tasks == oldSinceParameters.Tasks));
            }

            public void UpdatesSinceParametersOfTheFetchedEntity()
            {
                var oldAt = new DateTimeOffset(2017, 09, 01, 12, 34, 56, TimeSpan.Zero);
                var newAt = new DateTimeOffset(2017, 10, 01, 12, 34, 56, TimeSpan.Zero);
                var oldSinceParameters = new SinceParameters(null, oldAt);
                var entities = CreateListWithOneItem(newAt);
                var observables = CreateObservables(oldSinceParameters, entities);
                setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);

                state.Start(observables).SingleAsync().Wait();

                sinceParameterRepository.Received().Set(Arg.Is<ISinceParameters>(
                    newSinceParameters => OtherSinceDatesDidntChange(oldSinceParameters, newSinceParameters, newAt)));
            }

            public void HandlesNullValueReceivedFromTheServerAsAnEmptyList()
            {
                var oldSinceParameters = new SinceParameters(null);
                List<TInterface> entities = null;
                var observables = CreateObservables(oldSinceParameters, entities);

                var transition = (Transition<FetchObservables>)state.Start(observables).SingleAsync().Wait();

                transition.Result.Should().Be(state.FinishedPersisting);
                assertBatchUpdateWasCalled(new List<TInterface>());
            }

            public void SelectsTheLatestAtValue()
            {
                var oldSinceParameters = new SinceParameters(null);
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(oldSinceParameters, entities);
                setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);

                state.Start(observables).SingleAsync().Wait();

                sinceParameterRepository.Received().Set(Arg.Is<ISinceParameters>(
                    (newSinceParameters) => OtherSinceDatesDidntChange(oldSinceParameters, newSinceParameters, at)));
            }

            public void SinceDatesAreNotUpdatedWhenBatchUpdateThrows()
            {
                var oldSinceParameters = new SinceParameters(null, at);
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(oldSinceParameters, entities);
                setupBatchUpdateToThrow(new TestException());

                try
                {
                    state.Start(observables).SingleAsync().Wait();
                }
                catch (TestException) { }

                sinceParameterRepository.DidNotReceiveWithAnyArgs().Set(null);
            }

            public void PassesTheNewSinceParametersThroughTheTransition()
            {
                var oldSinceParameters = new SinceParameters(null, at);
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(oldSinceParameters, entities);
                setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);

                var transition = (Transition<FetchObservables>)state.Start(observables).SingleAsync().Wait();

                transition.Parameter.SinceParameters.Should()
                    .Match((ISinceParameters newSinceParameters) => OtherSinceDatesDidntChange(oldSinceParameters, newSinceParameters, at));
            }

            public void ReturnsFailedTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
            {
                var state = CreateState(repository, sinceParameterRepository);
                var observables = createFetchObservablesWhichThrow(exception);

                var transition = state.Start(observables).SingleAsync().Wait();
                var reason = ((Transition<Exception>)transition).Parameter;

                transition.Result.Should().Be(state.Failed);
                reason.Should().BeAssignableTo<ClientErrorException>();
            }

            public void ReturnsFailedTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
            {
                var state = CreateState(repository, sinceParameterRepository);
                var observables = createFetchObservablesWhichThrow(exception);

                var transition = state.Start(observables).SingleAsync().Wait();
                var reason = ((Transition<Exception>)transition).Parameter;

                transition.Result.Should().Be(state.Failed);
                reason.Should().BeAssignableTo<ServerErrorException>();
            }

            public void ThrowsWhenCertainExceptionsAreCaught(Exception exception)
            {
                var state = CreateState(repository, sinceParameterRepository);
                Exception caughtException = null;
                var observables = createFetchObservablesWhichThrow(exception);

                try
                {
                    state.Start(observables).Wait();
                }
                catch (Exception e)
                {
                    caughtException = e;
                }

                caughtException.Should().NotBeNull();
                caughtException.Should().BeAssignableTo(exception.GetType());
            }

            protected FetchObservables CreateFetchObservables(
                FetchObservables old, ISinceParameters sinceParameters,
                IObservable<List<IWorkspace>> workspaces = null,
                IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures = null,
                IObservable<IUser> user = null,
                IObservable<List<IClient>> clients = null,
                IObservable<List<IProject>> projects = null,
                IObservable<List<ITimeEntry>> timeEntries = null,
                IObservable<List<ITag>> tags = null,
                IObservable<List<ITask>> tasks = null,
                IObservable<IPreferences> preferences = null)
            => new FetchObservables(
                old?.SinceParameters ?? sinceParameters,
                old?.Workspaces ?? workspaces,
                old?.WorkspaceFeatures ?? workspaceFeatures,
                old?.User ?? user,
                old?.Clients ?? clients,
                old?.Projects ?? projects,
                old?.TimeEntries ?? timeEntries,
                old?.Tags ?? tags,
                old?.Tasks ?? tasks,
                old?.Preferences ?? preferences);

            protected abstract TState CreateState(IRepository<TDatabaseInterface> repository, ISinceParameterRepository sinceParameterRepository);

            protected abstract List<TInterface> CreateListWithOneItem(DateTimeOffset? at = null);

            protected abstract List<TInterface> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset? at);

            protected abstract FetchObservables CreateObservablesWhichFetchesTwice();

            protected abstract bool OtherSinceDatesDidntChange(ISinceParameters old, ISinceParameters next, DateTimeOffset at);

            protected abstract FetchObservables CreateObservables(ISinceParameters since = null, List<TInterface> entities = null);

            protected abstract bool IsDeletedOnServer(TInterface entity);

            protected abstract TDatabaseInterface Clean(TInterface entity);

            protected abstract Func<TDatabaseInterface, bool> ArePersistedAndClean(List<TInterface> entities);

            private void setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(List<TInterface> entities = null)
            {
                var foundationEntities = entities?.Select(entity => IsDeletedOnServer(entity)
                    ? (IConflictResolutionResult<TDatabaseInterface>)new DeleteResult<TDatabaseInterface>(0)
                    : new UpdateResult<TDatabaseInterface>(0, Clean(entity)));
                repository.BatchUpdate(null, null)
                    .ReturnsForAnyArgs(Observable.Return(foundationEntities));
            }

            private void setupBatchUpdateToThrow(Exception exception)
                => repository.BatchUpdate(null, null).ReturnsForAnyArgs(_ => throw exception);

            private void assertBatchUpdateWasCalled(List<TInterface> entities = null)
            {
                repository.Received().BatchUpdate(Arg.Is<IEnumerable<(long, TDatabaseInterface entity)>>(
                        list => list.Count() == entities.Count && list.Select(pair => pair.Item2).All(ArePersistedAndClean(entities))),
                    Arg.Any<Func<TDatabaseInterface, TDatabaseInterface, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<TDatabaseInterface>>());
            }

            private FetchObservables createFetchObservablesWhichThrow(Exception exception)
                => CreateFetchObservables(null, new SinceParameters(null, at),
                    Observable.Throw<List<IWorkspace>>(exception),
                    Observable.Throw<List<IWorkspaceFeatureCollection>>(exception),
                    Observable.Throw<IUser>(exception),
                    Observable.Throw<List<IClient>>(exception),
                    Observable.Throw<List<IProject>>(exception),
                    Observable.Throw<List<ITimeEntry>>(exception),
                    Observable.Throw<List<ITag>>(exception),
                    Observable.Throw<List<ITask>>(exception),
                    Observable.Throw<IPreferences>(exception));
        }
    }
}

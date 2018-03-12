using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Suggestions;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Xunit;
using TimeEntry = Toggl.Foundation.Models.TimeEntry;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SuggestionsViewModelTests
    {
        public abstract class SuggestionsViewModelTest : BaseViewModelTests<SuggestionsViewModel>
        {
            protected TestScheduler Scheduler { get; } = new TestScheduler();

            protected ISuggestionProviderContainer Container { get; } = Substitute.For<ISuggestionProviderContainer>();

            protected override SuggestionsViewModel CreateViewModel()
                => new SuggestionsViewModel(DataSource, InteractorFactory, Container);

            protected void SetProviders(params ISuggestionProvider[] providers)
            {
                Container.Providers.Returns(providers.ToList().AsReadOnly());
            }
        }

        public sealed class TheConstructor : SuggestionsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource, 
                bool useContainer, 
                bool useInteractorFactory)
            {
                var container = useContainer ? Container : null;
                var dataSource = useDataSource ? DataSource : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SuggestionsViewModel(dataSource, interactorFactory, container);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheSuggestionsProperty : SuggestionsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task WorksWithSeveralProviders()
            {
                var provider1 = Substitute.For<ISuggestionProvider>();
                var provider2 = Substitute.For<ISuggestionProvider>();
                var suggestion1 = createSuggestion("t1", 12, 9);
                var suggestion2 = createSuggestion("t2", 9, 12);
                var observable1 = Scheduler.CreateColdObservable(createRecorded(0, suggestion1));
                var observable2 = Scheduler.CreateColdObservable(createRecorded(1, suggestion2));
                provider1.GetSuggestions().Returns(observable1);
                provider2.GetSuggestions().Returns(observable2);
                SetProviders(provider1, provider2);

                await ViewModel.Initialize();
                Scheduler.AdvanceTo(1);

                ViewModel.Suggestions.Should().HaveCount(2)
                         .And.Contain(new[] { suggestion1, suggestion2 });
            }

            [Fact, LogIfTooSlow]
            public async Task WorksIfProviderHasMultipleSuggestions()
            {
                var scheduler = new TestScheduler();
                var provider = Substitute.For<ISuggestionProvider>();
                var suggestions = Enumerable.Range(1, 3).Select(createSuggestion).ToArray();
                var observableContent = suggestions
                    .Select(suggestion => createRecorded(1, suggestion))
                    .ToArray();
                var observable = scheduler.CreateColdObservable(observableContent);
                provider.GetSuggestions().Returns(observable);
                SetProviders(provider);

                await ViewModel.Initialize();
                scheduler.AdvanceTo(1);

                ViewModel.Suggestions.Should().HaveCount(suggestions.Length)
                         .And.Contain(suggestions);
            }

            [Fact, LogIfTooSlow]
            public async Task WorksIfProvidersAreEmpty()
            {
                var providers = Enumerable.Range(0, 3)
                    .Select(_ => Substitute.For<ISuggestionProvider>()).ToArray();

                foreach (var provider in providers)
                    provider.GetSuggestions().Returns(Observable.Empty<Suggestion>());

                SetProviders(providers);

                await ViewModel.Initialize();

                ViewModel.Suggestions.Should().HaveCount(0);
            }

            private Suggestion createSuggestion(int index) => createSuggestion($"te{index}", 0, 0);

            private Suggestion createSuggestion(string description, long taskId, long projectId) => new Suggestion(
                TimeEntry.Builder.Create(0)
                    .SetDescription(description)
                    .SetStart(DateTimeOffset.UtcNow)
                    .SetAt(DateTimeOffset.UtcNow)
                    .SetTaskId(taskId)
                    .SetProjectId(projectId)
                    .SetWorkspaceId(11)
                    .SetUserId(12)
                    .Build()
            );

            private Recorded<Notification<Suggestion>> createRecorded(int ticks, Suggestion suggestion)
                => new Recorded<Notification<Suggestion>>(ticks, Notification.CreateOnNext(suggestion));
        }

        public sealed class TheStartTimeEntryCommand : SuggestionsViewModelTest
        {
            public TheStartTimeEntryCommand()
            {
                var user = Substitute.For<IDatabaseUser>();
                user.Id.Returns(10);
                DataSource.User.Current.Returns(Observable.Return(user));

                TimeService.CurrentDateTime.Returns(DateTimeOffset.Now);
            }

            [Property]
            public void StarstATimeEntryWithTheSameValuesOfTheSelectedSuggestion(
                NonEmptyString description, long? projectId, long? taskId, long workspaceId)
            {
                if (workspaceId <= 0) return;

                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Description.Returns(description.Get);
                timeEntry.WorkspaceId.Returns(workspaceId);
                timeEntry.ProjectId.Returns(projectId);
                timeEntry.TaskId.Returns(taskId);
                timeEntry.Duration.Returns((long)TimeSpan.FromHours(1).TotalSeconds);
                var suggestion = new Suggestion(timeEntry);

                ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion).Wait();

                DataSource.TimeEntries.Received().Create(Arg.Is<IDatabaseTimeEntry>(dto =>
                    dto.Description == description.Get &&
                    dto.TaskId == taskId &&
                    dto.ProjectId == projectId &&
                    dto.WorkspaceId == workspaceId
                )).Wait();
            }

            [Fact, LogIfTooSlow]
            public async Task InitiatesPushSyncWhenStartingSucceeds()
            {
                var suggestion = createSuggestion();

                await ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion);

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotInitiatePushSyncWhenStartingFails()
            {
                var suggestion = createSuggestion();
                DataSource.TimeEntries.Create(Arg.Any<IDatabaseTimeEntry>())
                    .Returns(Observable.Throw<IDatabaseTimeEntry>(new Exception()));

                Action executeCommand
                    = () => ViewModel.StartTimeEntryCommand
                                .ExecuteAsync(suggestion)
                                .Wait();

                executeCommand.ShouldThrow<Exception>();
                await DataSource.SyncManager.DidNotReceive().PushSync();
            }

            [Fact, LogIfTooSlow]
            public async Task CannotBeExecutedTwiceInARow()
            {
                var suggestion = createSuggestion();
                DataSource.TimeEntries.Create(Arg.Any<IDatabaseTimeEntry>())
                    .Returns(Observable.Never<IDatabaseTimeEntry>());

                var _ = ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion);
                var __ = ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion);

                await DataSource.TimeEntries.Received(1).Create(Arg.Any<IDatabaseTimeEntry>());
            }

            [Fact, LogIfTooSlow]
            public async Task CanBeExecutedForTheSecondTimeIfStartingTheFirstOneFinishesSuccessfully()
            {
                var suggestion = createSuggestion();
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                DataSource.TimeEntries.Create(Arg.Any<IDatabaseTimeEntry>())
                    .Returns(Observable.Return(timeEntry));

                await ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion);
                await ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion);

                await DataSource.TimeEntries.Received(2).Create(Arg.Any<IDatabaseTimeEntry>());
            }

            [Fact]
            public async Task RegistersTheEventInTheAnalyticsService()
            {
                var suggestion = createSuggestion();

                await ViewModel.StartTimeEntryCommand.ExecuteAsync(suggestion);

                AnalyticsService.Received().TrackStartedTimeEntry(TimeEntryStartOrigin.Suggestion);
            }

            private Suggestion createSuggestion()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Duration.Returns((long)TimeSpan.FromMinutes(30).TotalSeconds);
                timeEntry.Description.Returns("Testing");
                timeEntry.WorkspaceId.Returns(10);
                return new Suggestion(timeEntry);
            }
        }
    }
}

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SuggestionsViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly ISuggestionProviderContainer suggestionProviders;
        private readonly ITimeService timeService;

        private IDisposable emptyDatabaseDisposable;

        private bool areStartButtonsEnabled = true;

        public MvxObservableCollection<Suggestion> Suggestions { get; }
            = new MvxObservableCollection<Suggestion>();

        public bool IsNewUser { get; private set; }

        public bool ShowWelcomeBack => !IsNewUser && !Suggestions.Any();

        public MvxAsyncCommand<Suggestion> StartTimeEntryCommand { get; set; }

        public SuggestionsViewModel(
            ITogglDataSource dataSource,
            IOnboardingStorage onboardingStorage,
            ISuggestionProviderContainer suggestionProviders,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.onboardingStorage = onboardingStorage;
            this.suggestionProviders = suggestionProviders;

            StartTimeEntryCommand = new MvxAsyncCommand<Suggestion>(startTimeEntry, _ => areStartButtonsEnabled);
        }

        public async override Task Initialize()
        {
            await base.Initialize();

            IsNewUser = onboardingStorage.IsNewUser();

            emptyDatabaseDisposable = dataSource.TimeEntries.IsEmpty
                .DistinctUntilChanged()
                .Subscribe(fetchSuggestions);
        }

        private void fetchSuggestions(bool databaseIsEmpty)
        {
            Suggestions.Clear();

            if (databaseIsEmpty) return;

            suggestionProviders
                .Providers
                .Select(provider => provider.GetSuggestions())
                .Aggregate(Observable.Merge)
                .Subscribe(addSuggestions);
        }

        private void addSuggestions(Suggestion suggestions)
        {
            Suggestions.Add(suggestions);
            IsNewUser = false;

            RaisePropertyChanged(nameof(IsNewUser));
            RaisePropertyChanged(nameof(ShowWelcomeBack));
        }

        private async Task startTimeEntry(Suggestion suggestion)
        {
            areStartButtonsEnabled = false;
            StartTimeEntryCommand.RaiseCanExecuteChanged();

            await dataSource.User
                .Current
                .Select(user => new StartTimeEntryDTO
                {
                    UserId = user.Id,
                    TaskId = suggestion.TaskId,
                    ProjectId = suggestion.ProjectId,
                    Description = suggestion.Description,
                    WorkspaceId = suggestion.WorkspaceId,
                    StartTime = timeService.CurrentDateTime
                })
                .SelectMany(dataSource.TimeEntries.Start)
                .Do(_ => dataSource.SyncManager.PushSync())
                .Do(_ =>
                {
                    areStartButtonsEnabled = true;
                    StartTimeEntryCommand.RaiseCanExecuteChanged();
                });
        }
    }
}

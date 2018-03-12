using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MainViewModel : MvxViewModel
    {
        private bool isStopButtonEnabled = false;

        private CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IUserPreferences userPreferences;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;

        public TimeSpan CurrentTimeEntryElapsedTime { get; private set; } = TimeSpan.Zero;

        private DateTimeOffset? currentTimeEntryStart;

        public long? CurrentTimeEntryId { get; private set; }

        public string CurrentTimeEntryDescription { get; private set; }

        public string CurrentTimeEntryProject { get; private set; }

        public string CurrentTimeEntryProjectColor { get; private set; }

        public string CurrentTimeEntryTask { get; private set; }

        public string CurrentTimeEntryClient { get; private set; }

        public SyncProgress SyncingProgress { get; private set; }

        [DependsOn(nameof(SyncingProgress))]
        public bool ShowSyncIndicator => SyncingProgress == SyncProgress.Syncing;

        public bool IsAddDescriptionLabelVisible =>
            string.IsNullOrEmpty(CurrentTimeEntryDescription)
            && string.IsNullOrEmpty(CurrentTimeEntryProject);

        public bool ShouldShowEmptyState
            => SuggestionsViewModel.IsEmpty
            && TimeEntriesLogViewModel.IsEmpty
            && IsWelcome;

        public bool ShouldShowWelcomeBack
            => SuggestionsViewModel.IsEmpty
            && TimeEntriesLogViewModel.IsEmpty
            && !IsWelcome;

        public bool IsWelcome => TimeEntriesLogViewModel.IsWelcome;

        public bool IsInManualMode { get; set; } = false;

        public TimeEntriesLogViewModel TimeEntriesLogViewModel { get; } = Mvx.IocConstruct<TimeEntriesLogViewModel>();

        public SuggestionsViewModel SuggestionsViewModel { get; } = Mvx.IocConstruct<SuggestionsViewModel>();

        public IMvxAsyncCommand StartTimeEntryCommand { get; }

        public IMvxAsyncCommand StopTimeEntryCommand { get; }

        public IMvxAsyncCommand EditTimeEntryCommand { get; }

        public IMvxAsyncCommand OpenSettingsCommand { get; }

        public IMvxAsyncCommand OpenReportsCommand { get; }

        public IMvxCommand RefreshCommand { get; }

        public IMvxCommand ToggleManualMode { get; }

        public MainViewModel(
            ITogglDataSource dataSource,
            ITimeService timeService,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService,
            IUserPreferences userPreferences)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.navigationService = navigationService;
            this.onboardingStorage = onboardingStorage;
            this.userPreferences = userPreferences;

            RefreshCommand = new MvxCommand(refresh);
            OpenReportsCommand = new MvxAsyncCommand(openReports);
            OpenSettingsCommand = new MvxAsyncCommand(openSettings);
            EditTimeEntryCommand = new MvxAsyncCommand(editTimeEntry, () => CurrentTimeEntryId.HasValue);
            StopTimeEntryCommand = new MvxAsyncCommand(stopTimeEntry, () => isStopButtonEnabled);
            StartTimeEntryCommand = new MvxAsyncCommand(startTimeEntry, () => CurrentTimeEntryId.HasValue == false);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await TimeEntriesLogViewModel.Initialize();
            await SuggestionsViewModel.Initialize();

            var tickDisposable = timeService
                .CurrentDateTimeObservable
                .Where(_ => currentTimeEntryStart != null)
                .Subscribe(currentTime => CurrentTimeEntryElapsedTime = currentTime - currentTimeEntryStart.Value);

            var currentlyRunningTimeEntryDisposable = dataSource
                .TimeEntries
                .CurrentlyRunningTimeEntry
                .Subscribe(setRunningEntry);
            
            var syncManagerDisposable = dataSource
                .SyncManager
                .ProgressObservable
                .Subscribe(progress => SyncingProgress = progress);

            var isEmptyChangedDisposable = dataSource
                .TimeEntries
                .TimeEntryUpdated
                .Select(te => te.Id)
                .Merge(dataSource.TimeEntries.TimeEntryDeleted)
                .Subscribe(_ =>
                {
                    RaisePropertyChanged(nameof(ShouldShowWelcomeBack));
                    RaisePropertyChanged(nameof(ShouldShowEmptyState));
                });

            disposeBag.Add(tickDisposable);
            disposeBag.Add(syncManagerDisposable);
            disposeBag.Add(isEmptyChangedDisposable);
            disposeBag.Add(currentlyRunningTimeEntryDisposable);
        }

        public override void ViewAppearing()
        {
            base.ViewAppearing();

            IsInManualMode = userPreferences.IsManualModeEnabled();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            navigationService.Navigate<SuggestionsViewModel>();
            navigationService.Navigate<TimeEntriesLogViewModel>();

            ChangePresentation(new CardVisibilityHint(CurrentTimeEntryId != null));
        }

        private void setRunningEntry(IDatabaseTimeEntry timeEntry)
        {
            CurrentTimeEntryId = timeEntry?.Id;
            currentTimeEntryStart = timeEntry?.Start;
            CurrentTimeEntryDescription = timeEntry?.Description ?? "";

            CurrentTimeEntryTask = timeEntry?.Task?.Name ?? "";
            CurrentTimeEntryProject = timeEntry?.Project?.Name ?? "";
            CurrentTimeEntryProjectColor = timeEntry?.Project?.Color ?? "";
            CurrentTimeEntryClient = timeEntry?.Project?.Client?.Name ?? "";

            ChangePresentation(new CardVisibilityHint(CurrentTimeEntryId != null));

            isStopButtonEnabled = timeEntry != null;

            StopTimeEntryCommand.RaiseCanExecuteChanged();
            StartTimeEntryCommand.RaiseCanExecuteChanged();
            EditTimeEntryCommand.RaiseCanExecuteChanged();
        }

        private void refresh()
        {
            dataSource.SyncManager.ForceFullSync();
        }

        private Task openSettings()
            => navigationService.Navigate<SettingsViewModel>();

        private async Task openReports()
        {
            var user = await dataSource.User.Current;
            await navigationService.Navigate<ReportsViewModel, long>(user.DefaultWorkspaceId);
        }

        private Task startTimeEntry()
        {
            var parameter = IsInManualMode
                ? StartTimeEntryParameters.ForManualMode(timeService.CurrentDateTime)
                : StartTimeEntryParameters.ForTimerMode(timeService.CurrentDateTime); 
            return navigationService.Navigate<StartTimeEntryViewModel, StartTimeEntryParameters>(parameter);
        }

        private async Task stopTimeEntry()
        {
            isStopButtonEnabled = false;
            StopTimeEntryCommand.RaiseCanExecuteChanged();

            await dataSource.TimeEntries.Stop(timeService.CurrentDateTime)
                .Do(_ => dataSource.SyncManager.PushSync());

            CurrentTimeEntryElapsedTime = TimeSpan.Zero;
        }

        private Task editTimeEntry()
            => navigationService.Navigate<EditTimeEntryViewModel, long>(CurrentTimeEntryId.Value);
    }
}

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class MainViewModel : MvxViewModel
    {
        private bool isWelcome = false;

        private bool isStopButtonEnabled = false;

        private CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
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

        public bool SpiderIsVisible { get; set; } = true;

        public IMvxAsyncCommand StartTimeEntryCommand { get; }

        public IMvxAsyncCommand StopTimeEntryCommand { get; }

        public IMvxAsyncCommand EditTimeEntryCommand { get; }

        public IMvxAsyncCommand OpenSettingsCommand { get; }

        public IMvxAsyncCommand OpenReportsCommand { get; }

        public IMvxCommand RefreshCommand { get; }

        public MainViewModel(
            ITogglDataSource dataSource,
            ITimeService timeService,
            IOnboardingStorage onboardingStorage,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.navigationService = navigationService;
            this.onboardingStorage = onboardingStorage;

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

            isWelcome = onboardingStorage.IsNewUser();

            var tickDisposable = timeService
                .CurrentDateTimeObservable
                .Where(_ => currentTimeEntryStart != null)
                .Subscribe(currentTime => CurrentTimeEntryElapsedTime = currentTime - currentTimeEntryStart.Value);

            var currentlyRunningTimeEntryDisposable = dataSource.TimeEntries
                .CurrentlyRunningTimeEntry
                .Subscribe(setRunningEntry);

            var syncManagerDisposable =
                dataSource.SyncManager.ProgressObservable
                    .Subscribe(progress => SyncingProgress = progress);

            var spiderDisposable =
                dataSource.TimeEntries.IsEmpty
                    .Subscribe(isEmpty => SpiderIsVisible = !isWelcome && isEmpty);

            disposeBag.Add(tickDisposable);
            disposeBag.Add(spiderDisposable);
            disposeBag.Add(syncManagerDisposable);
            disposeBag.Add(currentlyRunningTimeEntryDisposable);
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

        private Task startTimeEntry() =>
            navigationService.Navigate<StartTimeEntryViewModel, DateTimeOffset>(timeService.CurrentDateTime);

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

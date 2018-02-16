using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class TimeEntriesLogViewModel : MvxViewModel
    {
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IAnalyticsService analyticsService;
        private readonly IOnboardingStorage onboardingStorage;
        private readonly IMvxNavigationService navigationService;

        private bool areContineButtonsEnabled = true;

        public bool IsWelcome { get; private set; }

        public NestableObservableCollection<TimeEntryViewModelCollection, TimeEntryViewModel> TimeEntries { get; }
            = new NestableObservableCollection<TimeEntryViewModelCollection, TimeEntryViewModel>(
                newTimeEntry => vm => vm.Start < newTimeEntry.Start
            );

        [DependsOn(nameof(TimeEntries))]
        public bool IsEmpty => !TimeEntries.Any();

        [DependsOn(nameof(IsWelcome))]
        public string EmptyStateTitle
            => IsWelcome
            ? Resources.TimeEntriesLogEmptyStateWelcomeTitle
            : Resources.TimeEntriesLogEmptyStateTitle;

        [DependsOn(nameof(IsWelcome))]
        public string EmptyStateText
            => IsWelcome
            ? Resources.TimeEntriesLogEmptyStateWelcomeText
            : Resources.TimeEntriesLogEmptyStateText;

        public IMvxAsyncCommand<TimeEntryViewModel> EditCommand { get; }

        public IMvxAsyncCommand<TimeEntryViewModel> DeleteCommand { get; }

        public MvxAsyncCommand<TimeEntryViewModel> ContinueTimeEntryCommand { get; }

        public TimeEntriesLogViewModel(ITogglDataSource dataSource,
                                       ITimeService timeService,
                                       IAnalyticsService analyticsService,
                                       IOnboardingStorage onboardingStorage,
                                       IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
            this.onboardingStorage = onboardingStorage;
            this.navigationService = navigationService;

            EditCommand = new MvxAsyncCommand<TimeEntryViewModel>(edit);
            DeleteCommand = new MvxAsyncCommand<TimeEntryViewModel>(delete);
            ContinueTimeEntryCommand = new MvxAsyncCommand<TimeEntryViewModel>(continueTimeEntry, _ => areContineButtonsEnabled);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await reset();

            var deleteDisposable =
                dataSource.TimeEntries.TimeEntryDeleted
                          .Subscribe(safeRemoveTimeEntry);

            var updateDisposable =
                dataSource.TimeEntries.TimeEntryUpdated
                          .Subscribe(onTimeEntryUpdated);

            var createDisposable =
                dataSource.TimeEntries.TimeEntryCreated
                    .Where(isNotRunning)
                    .Subscribe(safeInsertTimeEntry);

            var midnightDisposable =
                timeService.MidnightObservable
                    .Subscribe(onMidnight);

            IsWelcome = onboardingStorage.IsNewUser();

            disposeBag.Add(createDisposable);
            disposeBag.Add(updateDisposable);
            disposeBag.Add(deleteDisposable);
            disposeBag.Add(midnightDisposable);
        }

        private async Task reset()
        {
            TimeEntries.Clear();

            var timeEntries = await dataSource.TimeEntries.GetAll();

            if (timeEntries == null) return;

            timeEntries
                .Where(isNotRunning)
                .OrderByDescending(te => te.Start)
                .Select(te => new TimeEntryViewModel(te))
                .GroupBy(te => te.Start.LocalDateTime.Date)
                .Select(grouping => new TimeEntryViewModelCollection(grouping.Key, grouping))
                .ForEach(addTimeEntries);
        }

        private void addTimeEntries(TimeEntryViewModelCollection collection)
        {
            IsWelcome = false;

            TimeEntries.Add(collection);

            RaisePropertyChanged(nameof(IsEmpty));
        }

        private void onTimeEntryUpdated((long Id, IDatabaseTimeEntry Entity) tuple)
        {
            var timeEntry = tuple.Entity;
            var shouldBeAdded = timeEntry != null && !timeEntry.IsRunning() && !timeEntry.IsDeleted;

            var oldCollectionIndex = TimeEntries.IndexOf(c => c.Any(vm => vm.Id == tuple.Id));
            var collectionIndex = TimeEntries.IndexOf(vm => vm.Date == timeEntry.Start.LocalDateTime.Date);
            var wasMovedIntoDifferentCollection = oldCollectionIndex >= 0 && oldCollectionIndex != collectionIndex;

            var shouldBeRemoved = shouldBeAdded == false || wasMovedIntoDifferentCollection;
            if (shouldBeRemoved)
            {
                safeRemoveTimeEntry(tuple.Id);
            }

            if (shouldBeAdded)
            {
                var timeEntryIndex = collectionIndex < 0 ? -1 : TimeEntries[collectionIndex].IndexOf(vm => vm.Id == tuple.Id);
                var timeEntryExistsInTheCollection = timeEntryIndex >= 0;
                if (timeEntryExistsInTheCollection)
                {
                    var timeEntryViewModel = new TimeEntryViewModel(timeEntry);
                    TimeEntries.ReplaceInChildCollection(collectionIndex, timeEntryIndex, timeEntryViewModel);
                    return;
                }

                safeInsertTimeEntry(timeEntry);
            }
        }

        private void safeInsertTimeEntry(IDatabaseTimeEntry timeEntry)
        {
            IsWelcome = false;

            var indexDate = timeEntry.Start.LocalDateTime.Date;
            var timeEntryViewModel = new TimeEntryViewModel(timeEntry);

            var collectionIndex = TimeEntries.IndexOf(x => x.Date == indexDate);
            if (collectionIndex >= 0)
            {
                TimeEntries.InsertInChildCollection(collectionIndex, timeEntryViewModel);
                return;
            }

            var newCollection = new TimeEntryViewModelCollection(indexDate, new[] { timeEntryViewModel });
            var foundIndex = TimeEntries.IndexOf(TimeEntries.FirstOrDefault(x => x.Date < indexDate));
            var indexToInsert = foundIndex == -1 ? TimeEntries.Count : foundIndex;
            TimeEntries.Insert(indexToInsert, newCollection);

            RaisePropertyChanged(nameof(IsEmpty));
        }

        private void safeRemoveTimeEntry(long id)
        {
            var collectionIndex = TimeEntries.IndexOf(c => c.Any(vm => vm.Id == id));
            if (collectionIndex < 0) return;

            var item = TimeEntries[collectionIndex].First(vm => vm.Id == id);
            TimeEntries.RemoveFromChildCollection(collectionIndex, item);
            RaisePropertyChanged(nameof(IsEmpty));
        }

        private void OnIsWelcomeChanged()
        {
            if (IsWelcome) return;

            onboardingStorage.SetIsNewUser(false);
        }

        private async void onMidnight(DateTimeOffset midnight)
            => await reset();

        private bool isNotRunning(IDatabaseTimeEntry timeEntry) => !timeEntry.IsRunning();

        private Task edit(TimeEntryViewModel timeEntryViewModel)
            => navigationService.Navigate<EditTimeEntryViewModel, long>(timeEntryViewModel.Id);

        private async Task delete(TimeEntryViewModel timeEntryViewModel)
        {
            await dataSource.TimeEntries.Delete(timeEntryViewModel.Id);
        }

        private async Task continueTimeEntry(TimeEntryViewModel timeEntryViewModel)
        {
            areContineButtonsEnabled = false;
            ContinueTimeEntryCommand.RaiseCanExecuteChanged();

            await dataSource.User
                .Current
                .Select(user => new StartTimeEntryDTO
                {
                    UserId = user.Id,
                    TaskId = timeEntryViewModel.TaskId,
                    WorkspaceId = timeEntryViewModel.WorkspaceId,
                    Billable = timeEntryViewModel.Billable,
                    StartTime = timeService.CurrentDateTime,
                    ProjectId = timeEntryViewModel.ProjectId,
                    Description = timeEntryViewModel.Description,
                    TagIds = timeEntryViewModel.TagIds
                })
                .SelectMany(dataSource.TimeEntries.Start)
                .Do(_ => dataSource.SyncManager.PushSync())
                .Do(_ =>
                {
                    areContineButtonsEnabled = true;
                    ContinueTimeEntryCommand.RaiseCanExecuteChanged();
                });

            analyticsService.TrackStartedTimeEntry(TimeEntryStartOrigin.Continue);
        }
    }
}

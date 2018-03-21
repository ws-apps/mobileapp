﻿using System;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Calendar;
using Toggl.Foundation.MvvmCross.ViewModels.Hints;
using Toggl.Foundation.Reports;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

[assembly: MvxNavigation(typeof(ReportsViewModel), ApplicationUrls.Reports)]
namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class ReportsViewModel : MvxViewModel<long>
    {
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IMvxNavigationService navigationService;
        private readonly ReportsCalendarViewModel calendarViewModel;
        private readonly Subject<Unit> reportSubject = new Subject<Unit>();
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        private DateTimeOffset startDate;
        private DateTimeOffset endDate;
        private long workspaceId;
        private DateFormat dateFormat;

        public bool IsLoading { get; private set; }

        public TimeSpan TotalTime { get; private set; } = TimeSpan.Zero;

        public DurationFormat DurationFormat { get; private set; }

        public bool TotalTimeIsZero => TotalTime.Ticks == 0;

        public float? BillablePercentage { get; private set; } = null;

        public MvxObservableCollection<ChartSegment> Segments { get; } = new MvxObservableCollection<ChartSegment>();

        public bool ShowEmptyState => !Segments.Any() && !IsLoading;

        public string CurrentDateRangeString { get; private set; }

        public bool IsCurrentWeek
        {
            get
            {
                var currentDate = timeService.CurrentDateTime.Date;
                var startOfWeek = currentDate.AddDays(1 - (int)currentDate.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                return startDate.Date == startOfWeek
                       && endDate.Date == endOfWeek;
            }
        }

        public IMvxCommand HideCalendarCommand { get; }

        public IMvxCommand ToggleCalendarCommand { get; }

        public IMvxCommand<DateRangeParameter> ChangeDateRangeCommand { get; }

        public ReportsViewModel(ITogglDataSource dataSource,
                                ITimeService timeService,
                                IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.timeService = timeService;
            this.navigationService = navigationService;
            this.dataSource = dataSource;

            calendarViewModel = new ReportsCalendarViewModel(timeService, dataSource);

            HideCalendarCommand = new MvxCommand(hideCalendar);
            ToggleCalendarCommand = new MvxCommand(toggleCalendar);
            ChangeDateRangeCommand = new MvxCommand<DateRangeParameter>(changeDateRange);
        }

        public override void Prepare(long parameter)
        {
            workspaceId = parameter;
        }

        public async override Task Initialize()
        {
            if (workspaceId == 0)
                workspaceId = await dataSource
                    .User
                    .Current
                    .Select(user => user.DefaultWorkspaceId);

            var currentDate = timeService.CurrentDateTime.Date;
            startDate = currentDate.AddDays(1 - (int)currentDate.DayOfWeek);
            endDate = startDate.AddDays(6);

            disposeBag.Add(
                reportSubject
                    .StartWith(Unit.Default)
                    .AsObservable()
                    .Do(setLoadingState)
                    .SelectMany(_ => dataSource.ReportsProvider.GetProjectSummary(workspaceId, startDate, endDate))
                    .Subscribe(onReport, onError)
            );

            disposeBag.Add(
                calendarViewModel.SelectedDateRangeObservable.Subscribe(
                    newDateRange => ChangeDateRangeCommand.Execute(newDateRange)
                )
            );

            var preferencesDisposable = dataSource.Preferences.Current
                .Subscribe(onPreferencesChanged);

            disposeBag.Add(preferencesDisposable);
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            navigationService.Navigate(calendarViewModel);
        }

        private void setLoadingState(Unit obj)
        {
            IsLoading = true;
            Segments.Clear();
        }

        private void onReport(ProjectSummaryReport report)
        {
            TotalTime = TimeSpan.FromSeconds(report.TotalSeconds);
            BillablePercentage = report.TotalSeconds == 0 ? null : (float?)report.BillablePercentage;

            var segments = report.Segments.Select(segment => segment.WithDurationFormat(DurationFormat));
            Segments.AddRange(segments);
            IsLoading = false;

            RaisePropertyChanged(nameof(Segments));
        }

        private void onError(Exception ex)
        {
            RaisePropertyChanged(nameof(Segments));
            IsLoading = false;
        }

        private void toggleCalendar()
        {
            ChangePresentation(new ToggleCalendarVisibilityHint());
            calendarViewModel.OnToggleCalendar();
        }

        private void hideCalendar()
        {
            ChangePresentation(new ToggleCalendarVisibilityHint(forceHide: true));
            calendarViewModel.OnHideCalendar();
        }

        private void changeDateRange(DateRangeParameter dateRange)
        {
            startDate = dateRange.StartDate;
            endDate = dateRange.EndDate;
            updateCurrentDateRangeString();
            reportSubject.OnNext(Unit.Default);
        }

        private void updateCurrentDateRangeString()
        {
            if (startDate == endDate)
            {
                CurrentDateRangeString = $"{startDate.ToString(dateFormat.Short)} ▾";
                return;
            }
            
            CurrentDateRangeString = IsCurrentWeek
                ? $"{Resources.ThisWeek} ▾"
                : $"{startDate.ToString(dateFormat.Short)} - {endDate.ToString(dateFormat.Short)} ▾";
        }

        private void onPreferencesChanged(IDatabasePreferences preferences)
        {
            DurationFormat = preferences.DurationFormat;
            dateFormat = preferences.DateFormat;

            var segments = Segments.Select(segment => segment.WithDurationFormat(DurationFormat));
            Segments.Clear();
            Segments.AddRange(segments);

            updateCurrentDateRangeString();
        }
    }
}

using System;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class EditDurationViewModel : MvxViewModel<DurationParameter, DurationParameter>
    {
        private readonly ITimeService timeService;
        private readonly IMvxNavigationService navigationService;
        private readonly ITogglDataSource dataSource;

        private IDisposable runningTimeEntryDisposable;
        private IDisposable preferencesDisposable;

        private DurationParameter defaultResult;

        private DurationFormat durationFormat;

        [DependsOn(nameof(IsRunning))]
        public DurationFormat DurationFormat => IsRunning ? DurationFormat.Improved : durationFormat;

        public DateFormat DateFormat { get; private set; }

        public TimeFormat TimeFormat { get; private set; }

        public bool IsRunning { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public DateTimeOffset StopTime { get; private set; }

        [DependsOn(nameof(StartTime), nameof(StopTime))]
        public TimeSpan Duration
        {
            get => StopTime - StartTime;
            set
            {
                if (Duration == value) return;

                onDurationChanged(value);
            }
        }

        [DependsOn(nameof(IsEditingStartTime), nameof(IsEditingStopTime))]
        public bool IsEditingTime => IsEditingStopTime || IsEditingStartTime;

        public bool IsEditingStartTime { get; private set; }

        public bool IsEditingStopTime { get; private set; }

        public DateTimeOffset EditedTime
        {
            get => IsEditingStartTime ? StartTime : StopTime;
            set
            {
                if (IsEditingTime == false) return;

                value = value.Clamp(MinimumDateTime, MaximumDateTime);

                if (IsEditingStartTime)
                {
                    StartTime = value;
                }
                else
                {
                    StopTime = value;
                }
            }
        }

        public DateTime MinimumDateTime { get; private set; }

        public DateTime MaximumDateTime { get; private set; }

        public DateTimeOffset MinimumStartTime => StopTime.AddHours(-MaxTimeEntryDurationInHours);

        public DateTimeOffset MaximumStartTime => StopTime;

        public DateTimeOffset MinimumStopTime => StartTime;

        public DateTimeOffset MaximumStopTime => StartTime.AddHours(MaxTimeEntryDurationInHours);

        public IMvxAsyncCommand SaveCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxCommand EditStartTimeCommand { get; }

        public IMvxCommand EditStopTimeCommand { get; }

        public IMvxCommand StopEditingTimeCommand { get; }

        public EditDurationViewModel(IMvxNavigationService navigationService, ITimeService timeService, ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.timeService = timeService;
            this.navigationService = navigationService;
            this.dataSource = dataSource;

            SaveCommand = new MvxAsyncCommand(save);
            CloseCommand = new MvxAsyncCommand(close);

            EditStartTimeCommand = new MvxCommand(editStartTime);
            EditStopTimeCommand = new MvxCommand(editStopTime);
            StopEditingTimeCommand = new MvxCommand(stopEditingTime);
        }

        public override void Prepare(DurationParameter parameter)
        {
            defaultResult = parameter;
            IsRunning = defaultResult.Duration.HasValue == false;

            if (IsRunning)
            {
                runningTimeEntryDisposable = timeService.CurrentDateTimeObservable
                           .Subscribe(currentTime => StopTime = currentTime);
            }

            StartTime = parameter.Start;
            StopTime = parameter.Duration.HasValue
                ? StartTime + parameter.Duration.Value
                : timeService.CurrentDateTime;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            preferencesDisposable = dataSource.Preferences.Current
                .Subscribe(onPreferencesChanged);
        }

        private Task close()
            => navigationService.Close(this, defaultResult);

        private Task save()
        {
            var result = DurationParameter.WithStartAndDuration(StartTime, IsRunning ? (TimeSpan?)null : Duration);
            return navigationService.Close(this, result);
        }

        private void editStartTime()
        {
            if (IsEditingStartTime == false)
            {
                MinimumDateTime = StopTime.AddHours(-MaxTimeEntryDurationInHours).LocalDateTime;
                MaximumDateTime = StopTime.LocalDateTime;

                IsEditingStartTime = true;
                IsEditingStopTime = false;

                EditedTime = StartTime;
            }
            else
            {
                IsEditingStartTime = false;
            }
        }

        private void editStopTime()
        {
            if (IsRunning)
            {
                runningTimeEntryDisposable?.Dispose();
                StopTime = timeService.CurrentDateTime;
                IsRunning = false;
            }

            if (IsEditingStopTime == false)
            {
                MinimumDateTime = StartTime.LocalDateTime;
                MaximumDateTime = StartTime.AddHours(MaxTimeEntryDurationInHours).LocalDateTime;

                IsEditingStopTime = true;
                IsEditingStartTime = false;

                EditedTime = StopTime;
            }
            else
            {
                IsEditingStopTime = false;
            }
        }

        private void stopEditingTime()
        {
            IsEditingStopTime = false;
            IsEditingStartTime = false;
        }

        private void onDurationChanged(TimeSpan changedDuration)
        {
            if (IsRunning)
                StartTime = timeService.CurrentDateTime - changedDuration;

            StopTime = StartTime + changedDuration;
        }

        private void onPreferencesChanged(IDatabasePreferences preferences)
        {
            durationFormat = preferences.DurationFormat;
            DateFormat = preferences.DateFormat;
            TimeFormat = preferences.TimeOfDayFormat;

            RaisePropertyChanged(nameof(DurationFormat));
        }
    }
}

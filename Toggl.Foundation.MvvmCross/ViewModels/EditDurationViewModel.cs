using System;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class EditDurationViewModel : MvxViewModel<DurationParameter, DurationParameter>
    {
        private readonly ITimeService timeService;
        private readonly IMvxNavigationService navigationService;

        private IDisposable runningTimeEntryDisposable;

        private DurationParameter defaultResult;

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

        public DateTime MinimumTime { get; private set; }

        public DateTime MaximumTime { get; private set; }

        public DateTimeOffset MinimumStartTime => StopTime.AddHours(-MaxTimeEntryDurationInHours);

        public DateTimeOffset MaximumStartTime => StopTime;

        public DateTimeOffset MinimumStopTime => StartTime;

        public DateTimeOffset MaximumStopTime => StartTime.AddHours(MaxTimeEntryDurationInHours);

        public IMvxAsyncCommand SaveCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxCommand StopTimeEntryCommand { get; }

        public EditDurationViewModel(IMvxNavigationService navigationService, ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.timeService = timeService;
            this.navigationService = navigationService;

            SaveCommand = new MvxAsyncCommand(save);
            CloseCommand = new MvxAsyncCommand(close);
            StopTimeEntryCommand = new MvxCommand(stopRunningTimeEntry);
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

        private Task close()
            => navigationService.Close(this, defaultResult);

        private Task save()
        {
            var result = DurationParameter.WithStartAndDuration(StartTime, IsRunning ? (TimeSpan?)null : Duration);
            return navigationService.Close(this, result);
        }

        private void stopRunningTimeEntry()
        {
            runningTimeEntryDisposable?.Dispose();
            StopTime = timeService.CurrentDateTime;
            IsRunning = false;
        }

        private void onDurationChanged(TimeSpan changedDuration)
        {
            if (IsRunning)
                StartTime = timeService.CurrentDateTime - changedDuration;

            StopTime = StartTime + changedDuration;
        }
    }
}

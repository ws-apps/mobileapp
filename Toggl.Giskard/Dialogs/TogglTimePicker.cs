using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using MvvmCross.Platform.WeakSubscription;

namespace Toggl.Giskard.Dialogs
{
    public sealed class TogglTimePicker
    {
        private readonly TaskCompletionSource<DateTimeOffset> completionSource = new TaskCompletionSource<DateTimeOffset>();
        private readonly Context context;
        private readonly DateTimeOffset date;
        private readonly bool is24hourView;
        private bool hasRun;

        public TogglTimePicker(Context context, DateTimeOffset? date, bool is24hourView = true)
        {
            this.context = context;
            this.date = date ?? DateTimeOffset.Now;
            this.is24hourView = is24hourView;
        }

        public Task<DateTimeOffset> Show()
        {
            if (hasRun)
                throw new InvalidOperationException("Show should only be called once.");

            hasRun = true;

            var dialog = new TimePickerDialog(
                context, onTimeSet,
                date.Hour, date.Minute, is24hourView);

            dialog.Show();

            return completionSource.Task;
        }

        private void onTimeSet(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            var selectedDate = new DateTimeOffset(
                date.Year, date.Month, date.Day,
                e.HourOfDay, e.Minute, date.Second, date.Offset);

            completionSource.SetResult(selectedDate);
        }
    }

    public static class TogglTimePickerExtensions
    {
        public static IDisposable BindTimePickerToClick(this View view, Func<DateTimeOffset?> initialValueSelector, Action<DateTimeOffset> onTimeSelected, bool is24hourView = true)
        {
            return view.WeakSubscribe(nameof(view.Click), async (sender, e) =>
            {
                onTimeSelected(await new TogglTimePicker(view.Context, initialValueSelector(), is24hourView).Show());
            });
        }
    }
}
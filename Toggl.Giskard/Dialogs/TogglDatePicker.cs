using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using MvvmCross.Platform.WeakSubscription;

namespace Toggl.Giskard.Dialogs
{
    public sealed class TogglDatePicker
    {
        private readonly TaskCompletionSource<DateTimeOffset> completionSource = new TaskCompletionSource<DateTimeOffset>();
        private readonly Context context;
        private readonly DateTimeOffset date;
        private bool hasRun;

        public TogglDatePicker(Context context, DateTimeOffset? date)
        {
            this.context = context;
            this.date = date?? DateTimeOffset.Now;
        }

        public Task<DateTimeOffset> Show()
        {
            if (hasRun) 
                throw new InvalidOperationException("Show should only be called once.");

            hasRun = true;

            var dialog = new DatePickerDialog(
                context, onDateSet, 
                date.Year, date.Month - 1, date.Day);

            dialog.Show();

            return completionSource.Task;
        }

        private void onDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            var selectedDate = new DateTimeOffset(
                e.Year, e.Month + 1, e.DayOfMonth,
                date.Hour, date.Minute, date.Second, date.Offset);

            completionSource.SetResult(selectedDate);
        }
    }

    public static class TogglDatePickerExtensions
    {
        public static IDisposable BindDatePickerToClick(this View view, Func<DateTimeOffset?> initialValueSelector, Action<DateTimeOffset> onTimeSelected)
        {
            return view.WeakSubscribe(nameof(view.Click), async (sender, e) =>
            {
                onTimeSelected(await new TogglDatePicker(view.Context, initialValueSelector()).Show());
            });
        }
    }
}

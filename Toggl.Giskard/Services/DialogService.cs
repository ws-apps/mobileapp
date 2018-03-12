using System;
using System.Threading.Tasks;
using Android.Support.V7.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Services;

namespace Toggl.Giskard.Services
{
    public sealed class DialogService : IDialogService
    {
        public Task<bool> Confirm(string title, string message, string confirmButtonText, string dismissButtonText)
        {
            var activity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
            var tcs = new TaskCompletionSource<bool>();

            MvxSingleton<IMvxMainThreadDispatcher>.Instance.RequestMainThreadAction(() =>
            {
                var builder = new AlertDialog.Builder(activity, Resource.Style.TogglDialog)
                    .SetMessage(message)
                    .SetPositiveButton(confirmButtonText, (s, e) => tcs.SetResult(true));

                if (!string.IsNullOrWhiteSpace(title))
                {
                    builder = builder.SetTitle(title);
                }

                if (!string.IsNullOrEmpty(dismissButtonText))
                {
                    builder = builder.SetNegativeButton(dismissButtonText, (s, e) => tcs.SetResult(false));
                }

                builder.Show();
            });

            return tcs.Task;
        }

        public Task Alert(string title, string message, string buttonTitle)
            => Confirm(title, message, buttonTitle, null);

        public Task<bool> ConfirmDestructiveAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.DiscardNewTimeEntry:
                    return Confirm(null, Resources.DiscardThisTimeEntry, Resources.Delete, Resources.Cancel);
                case ActionType.DeleteExistingTimeEntry:
                    return Confirm(null, Resources.DeleteThisTimeEntry, Resources.Discard, Resources.Cancel);
            }

            throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}

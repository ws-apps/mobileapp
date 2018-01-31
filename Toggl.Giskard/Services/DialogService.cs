using System.Threading.Tasks;
using Android.Support.V7.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.Droid.Platform;
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
                new AlertDialog.Builder(activity)
                    .SetMessage(message)
                    .SetTitle(title)
                    .SetPositiveButton(confirmButtonText, (s, e) => tcs.SetResult(true))
                    .SetNegativeButton(dismissButtonText, (s, e) => tcs.SetResult(false))
                    .Show();
            });

            return tcs.Task;
        }

        public Task<string> ShowMultipleChoiceDialog(string cancelText, params MultipleChoiceDialogAction[] actions)
        {
            throw new System.NotImplementedException();
        }

        Task<object> IDialogService.Alert(string title, string message, string buttonTitle)
        {
            throw new System.NotImplementedException();
        }
    }
}

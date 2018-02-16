using Android.App;
using Android.Graphics;
using Android.Views;
using Toggl.Giskard.Helper;

namespace Toggl.Giskard.Extensions
{
    public static class ActivityExtensions
    {
        public static void ChangeStatusBarColor(this Activity activity, Color color, bool useDarkIcons = false)
        {
            var window = activity.Window;
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            window.SetStatusBarColor(color);

            if (MarshmallowApis.AreNotAvailable) return;

            window.DecorView.SystemUiVisibility =
                (StatusBarVisibility)(useDarkIcons ? SystemUiFlags.LightStatusBar : SystemUiFlags.Visible);
        }
    }
}

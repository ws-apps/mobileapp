using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Views;

namespace Toggl.Giskard
{
    [Activity(Label = "Toggl.Giskard", MainLauncher = true, Icon = "@mipmap/icon",
        Theme = "@style/Theme.Splash", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashScreen : MvxSplashScreenActivity
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {
        }

        #if USE_ANALYTICS
        protected override void OnCreate(Android.OS.Bundle bundle)
        {
            base.OnCreate(bundle);

            Microsoft.AppCenter.AppCenter.Start(
                {TOGGL_APP_CENTER_ID_DROID},
                typeof(Microsoft.AppCenter.Crashes.Crashes));
        }
        #endif
    }
}

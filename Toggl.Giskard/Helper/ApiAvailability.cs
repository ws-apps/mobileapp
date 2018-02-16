using Android.OS;

namespace Toggl.Giskard.Helper
{
    public static class MarshmallowApis
    {
        public static bool AreAvailable
            => Build.VERSION.SdkInt >= BuildVersionCodes.M;

        public static bool AreNotAvailable
            => Build.VERSION.SdkInt < BuildVersionCodes.M;
    }
}

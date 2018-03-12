using System;
using Toggl.Foundation;

namespace Toggl.Giskard
{
    public sealed class PlatformConstants : IPlatformConstants
    {
        public string HelpUrl { get; } = "https://support.toggl.com/toggl-timer-for-android/";
        public string PhoneModel => throw new NotImplementedException();
        public string OperatingSystem => throw new NotImplementedException();
        public string FeedbackEmailSubject => throw new NotImplementedException();
    }
}

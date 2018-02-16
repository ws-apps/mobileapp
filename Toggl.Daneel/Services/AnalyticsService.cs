using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Foundation;
using Toggl.Foundation.Analytics;

namespace Toggl.Daneel.Services
{
    public sealed class AnalyticsService : BaseAnalyticsService
    {
        private const int maxAppCenterStringLength = 64;

        protected override void NativeTrackEvent(string eventName, Dictionary<string, string> parameters)
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(eventName, trimLongParameters(parameters));

            Analytics.LogEvent(new NSString(eventName), NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
                parameters.Values.ToArray(),
                parameters.Keys.ToArray()
            ));
        }

        private Dictionary<string, string> trimLongParameters(Dictionary<string, string> parameters)
        {
            var validParameters = new Dictionary<string, string>();
            foreach (var (key, value) in parameters)
            {
                validParameters.Add(trimForAppCenter(key), trimForAppCenter(value));
            }

            return validParameters;
        }

        private string trimForAppCenter(string text)
            => text.Length > maxAppCenterStringLength ? text.Substring(0, maxAppCenterStringLength) : text;
    }
}

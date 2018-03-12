using System.Collections.Generic;

namespace Toggl.Foundation.Services
{
    public interface ILicenseProvider
    {
        Dictionary<string, string> GetAppLicenses();
    }
}

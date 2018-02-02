using System.Collections.Generic;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Shortcuts
{
    public interface IApplicationShortcutCreator
    {
        void OnLogin();
        void OnLogout();
        void OnTimeEntryStarted(ITimeEntry timeEntry);
    }
}

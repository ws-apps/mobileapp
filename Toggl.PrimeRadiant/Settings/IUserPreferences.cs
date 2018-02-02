using System;

namespace Toggl.PrimeRadiant.Settings
{
    public interface IUserPreferences
    {
        bool IsManualModeEnabled();

        void EnableManualMode();

        void EnableTimerMode();

        void Reset();
    }
}

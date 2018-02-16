using System;
using System.Collections.Generic;
using System.Text;

namespace Toggl.Multivac.Models
{
    public interface IPreferences
    {
        string TimeOfDayFormat { get; }

        string DateFormat { get; }

        DurationFormat DurationFormat { get; }

        bool CollapseTimeEntries { get; }
    }
}

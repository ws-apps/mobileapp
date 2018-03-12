using System;
using Toggl.Foundation.DTOs;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface ITimeEntriesSource : IRepository<IDatabaseTimeEntry>
    {
        IObservable<IDatabaseTimeEntry> CurrentlyRunningTimeEntry { get; }

        IObservable<IDatabaseTimeEntry> TimeEntryCreated { get; }

        IObservable<(long Id, IDatabaseTimeEntry Entity)> TimeEntryUpdated { get; }

        IObservable<long> TimeEntryDeleted { get; }

        IObservable<bool> IsEmpty { get; }
         IObservable<IDatabaseTimeEntry> Stop(DateTimeOffset stopTime);

        IObservable<IDatabaseTimeEntry> Update(EditTimeEntryDto dto);
    }
}

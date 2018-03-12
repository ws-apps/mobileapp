using System;
using Toggl.Foundation.DTOs;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface IPreferencesSource : IRepository<IDatabasePreferences>
    {
        IObservable<IDatabasePreferences> Current { get; }

        IObservable<IDatabasePreferences> Get();

        IObservable<IDatabasePreferences> Update(EditPreferencesDTO dto);
    }
}

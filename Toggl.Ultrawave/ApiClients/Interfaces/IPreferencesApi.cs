using System;
using System.Reactive;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IPreferencesApi
    {
        IObservable<IPreferences> Get();
        IObservable<IPreferences> Update(IPreferences client);
    }
}

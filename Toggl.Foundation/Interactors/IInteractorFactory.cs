using System;
using Toggl.Foundation.Models;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    public interface IInteractorFactory
    {
        IInteractor<IObservable<IDatabaseTimeEntry>> CreateTimeEntry(ITimeEntryPrototype prototype);

        IInteractor<IObservable<IDatabaseTimeEntry>> StartSuggestion(Suggestion suggestion);

        IInteractor<IObservable<IDatabaseTimeEntry>> ContinueTimeEntry(ITimeEntryPrototype prototype);
    }
}

using System;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Collections;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class StartTimeEntrySuggestionsTemplateSelector : IMvxTemplateSelector
    {
        public const int Empty = 0;
        public const int NoEntityFound = 1;
        public const int TimeEntrySuggestion = 2;
        public const int ProjectSuggestion = 3;
        public const int TagSuggestion = 4;
        public const int TaskSuggestion = 5;
        public const int WorkspaceHeader = 6;

        public int GetItemLayoutId(int fromViewType)
        {
            switch (fromViewType)
            {
                case Empty:
                    return Resource.Layout.StartTimeEntryActivityEmptyCell;
                case NoEntityFound:
                    return Resource.Layout.StartTimeEntryActivityNoEntityCell;
                case TimeEntrySuggestion:
                    return Resource.Layout.StartTimeEntryActivityTimeEntryCell;
                case ProjectSuggestion:
                    return Resource.Layout.StartTimeEntryActivityProjectCell;
                case TagSuggestion:
                    return Resource.Layout.StartTimeEntryActivityTagCell;
                case TaskSuggestion:
                    return Resource.Layout.StartTimeEntryActivityTaskCell;
                case WorkspaceHeader:
                    return Resource.Layout.StartTimeEntryActivityWorkspaceHeader;
            }

            throw new ArgumentOutOfRangeException(nameof(fromViewType));
        }

        public int GetItemViewType(object forItemObject)
        {
            switch (forItemObject)
            {
                case TagSuggestion _: 
                    return TagSuggestion;
                case QuerySymbolSuggestion _: 
                    return Empty;
                case TaskSuggestion _: 
                    return TaskSuggestion;
                case NoEntityInfoMessage _:
                    return NoEntityFound;
                case ProjectSuggestion _: 
                    return ProjectSuggestion;
                case TimeEntrySuggestion _: 
                    return TimeEntrySuggestion;
                case WorkspaceGroupedCollection<AutocompleteSuggestion> _: 
                    return WorkspaceHeader;
            }

            throw new ArgumentException(nameof(forItemObject));
        }
    }
}

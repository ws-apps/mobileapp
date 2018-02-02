using System.Collections.Generic;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Suggestions;

namespace Toggl.Giskard
{
    public sealed class ApplicationShortcutCreator : BaseApplicationShortcutCreator
    {
        public ApplicationShortcutCreator(ISuggestionProviderContainer suggestionProviderContainer)
            : base(suggestionProviderContainer)
        {
        }

        protected override void ClearAllShortCuts()
        {
            //Not implemented
        }

        protected override void SetShortcuts(IEnumerable<ApplicationShortcut> shortcuts)
        {
            //Not implemented
        }
    }
}

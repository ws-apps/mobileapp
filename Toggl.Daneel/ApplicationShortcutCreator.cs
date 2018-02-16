using System.Collections.Generic;
using System.Linq;
using Foundation;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Suggestions;
using UIKit;

namespace Toggl.Daneel
{
    public sealed class ApplicationShortcutCreator : BaseApplicationShortcutCreator
    {
        private readonly IReadOnlyDictionary<ShortcutType, UIApplicationShortcutIcon> icons = new Dictionary<ShortcutType, UIApplicationShortcutIcon>
        {
            { ShortcutType.Reports, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Time) },
            { ShortcutType.StartTimeEntry, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Play) },
            { ShortcutType.TimeEntrySuggestion, UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Play) },
        };

        public ApplicationShortcutCreator(ISuggestionProviderContainer suggestionProviderContainer)
            : base(suggestionProviderContainer)
        {
        }

        protected override void ClearAllShortCuts()
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.ShortcutItems = new UIApplicationShortcutItem[0];
            });
        }

        protected override void SetShortcuts(IEnumerable<ApplicationShortcut> shortcuts)
        {
            //Temporary disable application shortcuts (until handling is implemented)
            return;

            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.ShortcutItems = shortcuts
                    .Select(createIosShortcut)
                    .ToArray();
            });
        }

        private UIApplicationShortcutItem createIosShortcut(ApplicationShortcut shortcut)
            => new UIApplicationShortcutItem(
                shortcut.Type.ToString(),
                shortcut.Title,
                shortcut.Subtitle,
                icons[shortcut.Type],
                userInfoFor(shortcut)
            );

        private NSDictionary<NSString, NSObject> userInfoFor(ApplicationShortcut shortcut)
            => new NSDictionary<NSString, NSObject>(new NSString("url"), new NSString(shortcut.Url));
    }
}

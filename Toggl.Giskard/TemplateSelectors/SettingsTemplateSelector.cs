using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SettingsTemplateSelector : MvxDefaultTemplateSelector
    {
        public SettingsTemplateSelector() : base(Resource.Layout.SettingsActivityWorkspaceCell) { }
    }
}

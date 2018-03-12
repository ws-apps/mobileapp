using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectWorkspaceTemplateSelector : MvxDefaultTemplateSelector
    {
        public SelectWorkspaceTemplateSelector() : base(Resource.Layout.SelectWorkspaceFragmentCell) { }
    }
}

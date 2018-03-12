using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectColorTemplateSelector : MvxDefaultTemplateSelector
    {
        public SelectColorTemplateSelector() : base(Resource.Layout.SelectColorFragmentCell) { }
    }
}

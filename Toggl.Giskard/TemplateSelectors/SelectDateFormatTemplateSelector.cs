using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectDateFormatTemplateSelector : MvxDefaultTemplateSelector
    {
        public SelectDateFormatTemplateSelector() : base(Resource.Layout.SelectDateFormatFragmentCell) { }
    }
}

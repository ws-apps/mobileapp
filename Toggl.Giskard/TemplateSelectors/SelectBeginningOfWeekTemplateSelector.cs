using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectBeginningOfWeekTemplateSelector : MvxDefaultTemplateSelector
    {
        public SelectBeginningOfWeekTemplateSelector()
            : base(Resource.Layout.SelectBeginningOfWeekFragmentCell) { }
    }
}

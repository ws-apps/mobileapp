using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SelectDurationFormatTemplateSelector : MvxDefaultTemplateSelector
    {
        public SelectDurationFormatTemplateSelector()
            : base(Resource.Layout.SelectDurationFormatFragmentCell)
        {
        }
    }
}

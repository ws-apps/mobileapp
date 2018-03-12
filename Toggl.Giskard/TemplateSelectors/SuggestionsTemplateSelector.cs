using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class SuggestionsTemplateSelector : MvxDefaultTemplateSelector
    {
        public SuggestionsTemplateSelector() : base(Resource.Layout.MainSuggestionsCard) { }
    }
}

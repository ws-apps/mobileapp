using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class ReportsTemplateSelector : IMvxTemplateSelector
    {
        public const int Summary = 0;
        public const int BarChart = 1;
        public const int PieChart = 2;
        public const int Detail = 3;

        public int GetItemLayoutId(int fromViewType)
            => Resource.Layout.ReportsActivitySummary;

        public int GetItemViewType(object forItemObject)
            => Summary;
    }
}

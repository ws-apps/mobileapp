using Toggl.Foundation.MvvmCross.Converters;

// Use this file to register overrides of BoolToConstantValueConverter.
// They are all single line classes, but an override is needed to avoid using the Fluent syntax.
namespace Toggl.Giskard.Converters
{
    public sealed class LogDescriptionTopOffsetValueConverter : BoolToConstantValueConverter<int>
    {
        public LogDescriptionTopOffsetValueConverter() : base(16, 27) { }
    }

    public sealed class ProjectDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public ProjectDrawableValueConverter() : base(Resource.Drawable.te_project_active, Resource.Drawable.project) { }
    }

    public sealed class TagsDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public TagsDrawableValueConverter() : base(Resource.Drawable.te_tag_active, Resource.Drawable.tag) { }
    }

    public sealed class BillableDrawableValueConverter : BoolToConstantValueConverter<int>
    {
        public BillableDrawableValueConverter() : base(Resource.Drawable.te_billable_active, Resource.Drawable.billable) { }
    }
}

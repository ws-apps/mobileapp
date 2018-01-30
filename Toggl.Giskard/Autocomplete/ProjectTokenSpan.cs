using Android.Graphics;
using Android.Support.V4.Graphics;

namespace Toggl.Giskard.Autocomplete
{
    public sealed class ProjectTokenSpan : TokenSpan
    {
        public ProjectTokenSpan(Color projectColor)
            : base(new Color(ColorUtils.SetAlphaComponent(projectColor.ToArgb(), 30)), projectColor, false)
        {
        }
    }
}

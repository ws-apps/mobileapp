using Android.Graphics;
using MvvmCross.Plugins.Color.Droid;
using FoundationColor = Toggl.Foundation.MvvmCross.Helper.Color;

namespace Toggl.Giskard.Autocomplete
{
    public sealed class TagsTokenSpan : TokenSpan
    {
        private static readonly Color tagsColor = FoundationColor.StartTimeEntry.TokenBorder.ToAndroidColor();

        public int TagIndex { get; }

        public TagsTokenSpan(int tagIndex)
            : base(tagsColor, tagsColor, true)
        {
            TagIndex = tagIndex;
        }
    }
}

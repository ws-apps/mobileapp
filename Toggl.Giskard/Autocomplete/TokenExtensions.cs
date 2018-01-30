using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using Java.Lang;
using Toggl.Foundation.Autocomplete;
using Toggl.Giskard.Autocomplete;

namespace Toggl.Giskard.Extensions
{
    public static class TokenExtensions
    {
        private const float spanSizeProportion = 0.8f;

        public static string GetDescription(this EditText self)
        {
            var spannable = self.TextFormatted as SpannableStringBuilder;
            var nextTransition = spannable.NextSpanTransition(0, spannable.Length(), Class.FromType(typeof(CharacterStyle)));

            var isNextTransitionNormalText = !spannable.GetSpans(0, nextTransition, Class.FromType(typeof(TokenSpan))).Any();
            return isNextTransitionNormalText ? self.TextFormatted.SubSequence(0, nextTransition) : "";
        }

        public static ISpannable GetSpannableText(this TextFieldInfo self)
        {
            var builder = new SpannableStringBuilder();

            builder.Append(self.Text);
            builder.AppendProjectToken(self);
            builder.AppendTagTokens(self);

            return builder;
        }

        public static void AppendProjectToken(this SpannableStringBuilder self, TextFieldInfo textFieldInfo)
        {
            if (textFieldInfo.ProjectId == null) return;

            var start = self.Length();
            self.Append(textFieldInfo.ProjectName);
            var end = self.Length();

            var projectColor = Color.ParseColor(textFieldInfo.ProjectColor);
            self.SetSpan(new ProjectTokenSpan(projectColor), start, end, SpanTypes.ExclusiveExclusive);
            self.SetSpan(new RelativeSizeSpan(spanSizeProportion), start, end, SpanTypes.ExclusiveExclusive);
        }

        public static void AppendTagTokens(this SpannableStringBuilder self, TextFieldInfo textFieldInfo)
        {
            for (int i = 0; i < textFieldInfo.Tags.Length; i++)
            {
                var tag = textFieldInfo.Tags[i];
                var start = self.Length();
                self.Append(tag.Name);
                var end = self.Length();

                self.SetSpan(new RelativeSizeSpan(spanSizeProportion), start, end, SpanTypes.ExclusiveExclusive);
                self.SetSpan(new TagsTokenSpan(i), start, end, SpanTypes.ExclusiveExclusive);
            }
        }
    }
}

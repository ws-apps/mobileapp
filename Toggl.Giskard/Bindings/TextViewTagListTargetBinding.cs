using System;
using System.Collections.Generic;
using System.Linq;
using Android.Text;
using Android.Text.Style;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Giskard.Autocomplete;

namespace Toggl.Giskard.Bindings
{
    public sealed class TextViewTagListTargetBinding
        : MvxTargetBinding<TextView, IEnumerable<string>>
    {
        private const float spanSizeProportion = 0.8f;

        public const string BindingName = "Tags";

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public TextViewTagListTargetBinding(TextView target) : base(target)
        {
        }

        protected override void SetValue(IEnumerable<string> value)
        {
            var tags = value?.ToList();

            if (tags == null || !tags.Any())
                return;

            var builder = new SpannableStringBuilder();

            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];

                var start = builder.Length();
                builder.Append(tag);
                var end = builder.Length();

                builder.SetSpan(new RelativeSizeSpan(spanSizeProportion), start, end, SpanTypes.ExclusiveExclusive);
                builder.SetSpan(new TagsTokenSpan(i), start, end, SpanTypes.ExclusiveExclusive);
            }

            Target.TextFormatted = builder;
            Target.Invalidate();
        }
    }
}

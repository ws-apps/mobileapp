﻿using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target;
using Toggl.Daneel.Autocomplete;
using UIKit;
using static Toggl.Multivac.Extensions.EnumerableExtensions;

namespace Toggl.Daneel.Binding
{
    public sealed class TextViewTagListTargetBinding : MvxTargetBinding<UITextView, IEnumerable<string>>
    {
        private const int tokenLeftMargin = 3;
        private const int tokenRightMargin = 3;

        public const string BindingName = "Tags";

        private readonly nfloat elipsisTagWidth;
        private readonly TokenTextAttachment elipsisTag;

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        public TextViewTagListTargetBinding(UITextView target) : base(target)
        {
            elipsisTag = "...".GetTagToken(tokenLeftMargin, tokenRightMargin);
            elipsisTagWidth = elipsisTag.Image.Size.Width;
        }

        protected override void SetValue(IEnumerable<string> value)
        {
            var tagTokens = value.GetTagTokens(tokenLeftMargin, tokenRightMargin);

            nfloat totalLength = 0;
            var cumulativeLengths = tagTokens
                .Select(token => totalLength += token.Image.Size.Width)
                .ToList();
            
            if (totalLength <= Target.ContentSize.Width)
            {
                Target.AttributedText = createAttributedString(tagTokens);
                return;
            }

            var tagCount = cumulativeLengths
                .IndexOf(length => length + elipsisTagWidth >= Target.Bounds.Width);

            if (tagCount == -1)
            {
                Target.AttributedText = createAttributedString(
                    new[] { elipsisTag });
                return;
            }

            Target.AttributedText = createAttributedString(
                tagTokens.Take(tagCount)
                .ToList()
                .Append(elipsisTag)
            );
        }

        private static NSAttributedString createAttributedString(
            IEnumerable<TokenTextAttachment> tagTokens)
        {
            var tagTokenString = tagTokens.Aggregate(
                new NSMutableAttributedString(),
                (result, token) =>
                {
                    result.Append(NSAttributedString.FromAttachment(token));
                    return result;
                }
            );

            return tagTokenString;
        }
    }
}

﻿using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using Toggl.Daneel.Extensions;
using static Toggl.Foundation.MvvmCross.Helper.Animation;
using static Toggl.Multivac.Math;

namespace Toggl.Daneel.Views
{
    [Register(nameof(ActivityIndicatorView))]
    public sealed class ActivityIndicatorView : UIImageView
    {
        private const float animationDuration = 1f;

        private string imageResource = "icLoader";
        public string ImageResource
        {
            get => imageResource;
            set => Image = UIImage.FromBundle(imageResource = value);
        }

        public ActivityIndicatorView(IntPtr handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            init();
        }

        public void StartAnimation()
        {
            var animation = createAnimation();
            Layer.RemoveAllAnimations();
            Layer.AddAnimation(animation, "spinning");
        }

        public void StopAnimation()
        {
            Layer.RemoveAllAnimations();
        }

        private void init()
        {
            Image = UIImage.FromBundle(ImageResource);
            ContentMode = UIViewContentMode.Center;
        }

        private CAAnimation createAnimation()
        {
            var animation = CABasicAnimation.FromKeyPath("transform.rotation.z");
            animation.Duration = animationDuration;
            animation.TimingFunction = Curves.Linear.ToMediaTimingFunction();
            animation.Cumulative = true;
            animation.From = NSNumber.FromNFloat(0);
            animation.To = NSNumber.FromNFloat((nfloat)FullCircle);
            animation.RepeatCount = float.PositiveInfinity;
            return animation;
        }
    }
}

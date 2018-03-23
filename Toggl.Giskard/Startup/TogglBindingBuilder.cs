﻿using Toggl.Foundation.MvvmCross.Combiners;
using MvvmCross.Binding.Droid;

namespace Toggl.Giskard
{
    public sealed class TogglBindingBuilder : MvxAndroidBindingBuilder
    {
        protected override void FillValueCombiners(MvvmCross.Binding.Combiners.IMvxValueCombinerRegistry registry)
        {
            registry.AddOrOverwrite("Duration", new DurationValueCombiner());
            base.FillValueCombiners(registry);
        }
    }
}

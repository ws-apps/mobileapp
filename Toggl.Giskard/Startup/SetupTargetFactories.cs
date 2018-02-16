﻿using Android.Views;
using MvvmCross.Binding.Bindings.Target.Construction;
using Toggl.Giskard.Bindings;
using Android.Widget;
using Android.Support.Design.Widget;

namespace Toggl.Giskard
{
    public partial class Setup
    {
        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            registry.RegisterCustomBindingFactory<View>(
                $"Left{ViewMarginTargetBinding.BindingName}",
                view => new ViewMarginTargetBinding(view, ViewMarginTargetBinding.BoundMargin.Left)
            );
            registry.RegisterCustomBindingFactory<View>(
                $"Top{ViewMarginTargetBinding.BindingName}",
                view => new ViewMarginTargetBinding(view, ViewMarginTargetBinding.BoundMargin.Top)
            );
            registry.RegisterCustomBindingFactory<View>(
                $"Right{ViewMarginTargetBinding.BindingName}",
                view => new ViewMarginTargetBinding(view, ViewMarginTargetBinding.BoundMargin.Right)
            );
            registry.RegisterCustomBindingFactory<View>(
                $"Bottom{ViewMarginTargetBinding.BindingName}",
                view => new ViewMarginTargetBinding(view, ViewMarginTargetBinding.BoundMargin.Bottom)
            );

            registry.RegisterCustomBindingFactory<View>(
                DrawableColorTargetBinding.BindingName,
                view => new DrawableColorTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<EditText>(
                EditTextFocusTargetBinding.BindingName,
                view => new EditTextFocusTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<TextView>(
                TextViewTagListTargetBinding.BindingName,
                view => new TextViewTagListTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<View>(
                ViewMarginTargetBinding.BindingName,
                view => new ViewMarginTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<FloatingActionButton>(
                FabVisibilityTargetBinding.BindingName,
                view => new FabVisibilityTargetBinding(view)
            );
        }
    }
}

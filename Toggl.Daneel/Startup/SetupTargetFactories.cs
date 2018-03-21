﻿﻿using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.iOS.Platform;
using Toggl.Daneel.Binding;
using UIKit;
using Toggl.Daneel.Views;
using System.Collections.Generic;
using MvvmCross.Binding.iOS.Views;

namespace Toggl.Daneel
{
    public partial class Setup : MvxIosSetup
    {
        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);

            registry.RegisterCustomBindingFactory<AutocompleteTextView>(
                AutocompleteTextViewTextInfoTargetBinding.BindingName,
                view => new AutocompleteTextViewTextInfoTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIBarButtonItem>(
                BarButtonAnimatedEnabledTargetBinding.BindingName,
                view => new BarButtonAnimatedEnabledTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIBarButtonItem>(
                BarButtonCommandTargetBinding.BindingName,
                view => new BarButtonCommandTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIButton>(
                ButtonImageTargetBinding.BindingName,
                view => new ButtonImageTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIDatePicker>(
                DatePickerDateTimeOffsetTargetBinding.BindingName,
                view => new DatePickerDateTimeOffsetTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIImageView>(
                ImageViewImageTargetBinding.BindingName,
                view => new ImageViewImageTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<NSLayoutConstraint>(
                LayoutConstraintAnimatedConstantTargetBinding.BindingName,
                view => new LayoutConstraintAnimatedConstantTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<NSLayoutConstraint>(
                LayoutConstraintConstantTargetBinding.BindingName,
                view => new LayoutConstraintConstantTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UINavigationItem>(
                NavigationItemHidesBackButtonTargetBinding.BindingName,
                view => new NavigationItemHidesBackButtonTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIScrollView>(
                ScrollViewAnimatedCurrentPageTargetBinding.BindingName,
                view => new ScrollViewAnimatedCurrentPageTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIScrollView>(
                ScrollViewCurrentPageTargetBinding.BindingName,
                view => new ScrollViewCurrentPageTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<SpiderOnARopeView>(
                SpiderOnARopeViewIsVisibleTargetBinding.BindingName,
                view => new SpiderOnARopeViewIsVisibleTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UISwitch>(
                SwitchAnimatedOnTargetBinding.BindingName,
                view => new SwitchAnimatedOnTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UITextField>(
                TextFieldFocusTargetBinding.BindingName,
                view => new TextFieldFocusTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UITextField>(
                TextFieldPlaceholderTargetBinding.BindingName,
                view => new TextFieldPlaceholderTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UITextField>(
                TextFieldSecureTextEntryTargetBinding.BindingName,
                view => new TextFieldSecureTextEntryTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UITextView>(
                TextViewTagListTargetBinding.BindingName,
                view => new TextViewTagListTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<TextViewWithPlaceholder>(
                TextViewWithPlaceholderTextTargetBinding.BindingName,
                view => new TextViewWithPlaceholderTextTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIView>(
                ViewAnimatedBackgroundGradientTargetBinding.BindingName,
                view => new ViewAnimatedBackgroundGradientTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIView>(
                ViewAnimatedBackgroundTargetBinding.BindingName,
                view => new ViewAnimatedBackgroundTargetBinding(view)
            );

            registry.RegisterCustomBindingFactory<UIView>(
                ViewAnimatedVisibilityTargetBinding.BindingName,
                view => new ViewAnimatedVisibilityTargetBinding(view)
            );
        }
    }
}

﻿﻿using Toggl.Daneel.Binding;
using UIKit;
using Toggl.Daneel.Views;
using MvvmCross.Binding.iOS.Views;
using System.Collections.Generic;

namespace Toggl.Daneel.Extensions
{  
    public static class ViewBindingExtensions
    {

        public static string BindTextFieldInfo(this AutocompleteTextView self)
            => AutocompleteTextViewTextInfoTargetBinding.BindingName;

        public static string BindAnimatedEnabled(this UIBarButtonItem self)
            => BarButtonAnimatedEnabledTargetBinding.BindingName;

        public static string BindCommand(this UIBarButtonItem self)
            => BarButtonCommandTargetBinding.BindingName;

        public static string BindImage(this UIButton self)
            => ButtonImageTargetBinding.BindingName;

        public static string BindDateTimeOffset(this UIDatePicker self)
            => DatePickerDateTimeOffsetTargetBinding.BindingName;

        public static string BindAnimatedConstant(this NSLayoutConstraint self)
            => LayoutConstraintAnimatedConstantTargetBinding.BindingName;

        public static string BindConstant(this NSLayoutConstraint self)
            => LayoutConstraintConstantTargetBinding.BindingName;

        public static string BindHidesBackButton(this UINavigationItem self)
            => NavigationItemHidesBackButtonTargetBinding.BindingName;

        public static string BindAnimatedCurrentPage(this UIScrollView self)
            => ScrollViewAnimatedCurrentPageTargetBinding.BindingName;

        public static string BindCurrentPage(this UIScrollView self)
            => ScrollViewCurrentPageTargetBinding.BindingName;

        public static string BindSpiderVisibility(this SpiderOnARopeView self)
            => SpiderOnARopeViewIsVisibleTargetBinding.BindingName;

        public static string BindAnimatedOn(this UISwitch self)
            => SwitchAnimatedOnTargetBinding.BindingName;

        public static string BindFocus(this UITextField self)
            => TextFieldFocusTargetBinding.BindingName;

        public static string BindPlaceholder(this UITextField self)
            => TextFieldPlaceholderTargetBinding.BindingName;

        public static string BindSecureTextEntry(this UITextField self)
            => TextFieldSecureTextEntryTargetBinding.BindingName;

        public static string BindTags(this UITextView self)
            => TextViewTagListTargetBinding.BindingName;

        public static string BindText(this TextViewWithPlaceholder self)
            => TextViewWithPlaceholderTextTargetBinding.BindingName;

        public static string BindAnimatedBackground(this UIView self)
            => ViewAnimatedBackgroundTargetBinding.BindingName;

        public static string BindAnimatedVisibility(this UIView self)
            => ViewAnimatedVisibilityTargetBinding.BindingName;
    }
}

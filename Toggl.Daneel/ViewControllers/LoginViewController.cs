using System;
using CoreText;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS;
using MvvmCross.iOS.Views.Presenters.Attributes;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Extensions;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public partial class LoginViewController : KeyboardAwareViewController<LoginViewModel>
    {
        private const int forgotPasswordLabelOffset = 27;

        private readonly UIBarButtonItem backButton =
            new UIBarButtonItem { Title = Resources.LoginBackButton, TintColor = UIColor.White };
        
        private readonly UIBarButtonItem nextButton =
            new UIBarButtonItem { Title = Resources.LoginNextButton, TintColor = UIColor.White };

        private IDisposable willEnterForegroundNotification;

        public LoginViewController() 
            : base(nameof(LoginViewController))
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing == false) return;

            willEnterForegroundNotification?.Dispose();
            willEnterForegroundNotification = null;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
  
            prepareViews();

            var invertedBoolConverter = new BoolToConstantValueConverter<bool>(false, true);

            var bindingSet = this.CreateBindingSet<LoginViewController, LoginViewModel>();

            //Text
            bindingSet.Bind(EmailTextField)
                      .To(vm => vm.Email)
                      .WithConversion(new EmailToStringValueConverter());

            bindingSet.Bind(PasswordTextField)
                      .To(vm => vm.Password)
                      .WithConversion(new PasswordToStringValueConverter());

            bindingSet.Bind(InfoLabel).To(vm => vm.InfoText);
            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindSecureTextEntry())
                      .To(vm => vm.IsPasswordMasked);

            bindingSet.Bind(GoogleSignInButton)
                      .For(v => v.BindTitle())
                      .To(vm => vm.GoogleButtonText);

            bindingSet.Bind(this)
                      .For(v => v.Title)
                      .To(vm => vm.Title);

            //Commands
            bindingSet.Bind(GoogleSignInButton).To(vm => vm.GoogleLoginCommand);
            bindingSet.Bind(ForgotPasswordButton).To(vm => vm.ForgotPasswordCommand);
            bindingSet.Bind(PrivacyPolicyButton).To(vm => vm.OpenPrivacyPolicyCommand);
            bindingSet.Bind(TermsOfServiceButton).To(vm => vm.OpenTermsOfServiceCommand);

            bindingSet.Bind(backButton)
                      .For(v => v.BindCommand())
                      .To(vm => vm.BackCommand);

            bindingSet.Bind(nextButton)
                      .For(v => v.BindCommand())
                      .To(vm => vm.NextCommand);

            bindingSet.Bind(ShowPasswordButton)
                      .For(v => v.BindTap())
                      .To(vm => vm.TogglePasswordVisibilityCommand);

            bindingSet.Bind(PasswordManagerButton)
                      .For(v => v.BindTap())
                      .To(vm => vm.StartPasswordManagerCommand);

            //Enabled
            bindingSet.Bind(EmailTextField)
                      .For(v => v.BindShouldReturn())
                      .To(vm => vm.NextCommand);
            
            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindShouldReturn())
                      .To(vm => vm.NextCommand);

            bindingSet.Bind(nextButton)
                      .For(v => v.Enabled)
                      .To(vm => vm.NextIsEnabled);

            bindingSet.Bind(backButton)
                      .For(v => v.Enabled)
                      .To(vm => vm.IsLoading)
                      .WithConversion(invertedBoolConverter);

            bindingSet.Bind(ForgotPasswordButton)
                      .For(v => v.Enabled)
                      .To(vm => vm.IsLoading)
                      .WithConversion(invertedBoolConverter);

            //Visibility
            bindingSet.Bind(EmailTextField)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.EmailFieldVisible);

            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.IsPasswordPage);

            bindingSet.Bind(ShowPasswordButton)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.ShowPasswordButtonVisible);

            bindingSet.Bind(InfoLabel)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.HasInfoText);

            bindingSet.Bind(ActivityIndicator)
                      .For(v => v.BindVisible())
                      .To(vm => vm.IsLoading);

            bindingSet.Bind(ForgotPasswordButton)
                      .For(v => v.BindVisible())
                      .To(vm => vm.ShowForgotPassword);

            bindingSet.Bind(SignUpLabels)
                      .For(v => v.BindVisible())
                      .To(vm => vm.IsSignUp);
            
            bindingSet.Bind(PasswordManagerButton)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.PasswordManagerVisible);
            //State
            bindingSet.Bind(EmailTextField)
                      .For(v => v.BindFocus())
                      .To(vm => vm.IsEmailFocused);
            
            bindingSet.Bind(PasswordTextField)
                      .For(v => v.BindFocus())
                      .To(vm => vm.IsPasswordPage);

            bindingSet.Apply();

            EmailTextField.BecomeFirstResponder();

            willEnterForegroundNotification = UIApplication.Notifications.ObserveWillEnterForeground((sender, e) => startAnimations());
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.UserInteractionEnabled = true;
            startAnimations();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.Constant = e.FrameEnd.Height + forgotPasswordLabelOffset;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            BottomConstraint.Constant = forgotPasswordLabelOffset;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        private void prepareViews()
        {
            prepareTextFields();
            prepareNavigationBar();

            ForgotPasswordButton.SetTitleColor(
                Color.Login.DisabledButtonColor.ToNativeColor(),
                UIControlState.Disabled);
        }

        private void prepareTextFields()
        {
            var placeholderAttributes = new CTStringAttributes(
                new UIStringAttributes { ForegroundColor = UIColor.White.ColorWithAlpha(0.5f) }.Dictionary
            );

            EmailTextField.ReturnKeyType = UIReturnKeyType.Next;
            EmailTextField.TintColor = UIColor.White;
            EmailTextField.AttributedPlaceholder = 
                new NSAttributedString(Resources.LoginSignUpEmailPlaceholder, placeholderAttributes);

            PasswordTextField.ReturnKeyType = UIReturnKeyType.Next;
            PasswordTextField.TintColor = UIColor.White;
            PasswordTextField.AttributedPlaceholder = 
                new NSAttributedString(Resources.LoginSignUpPasswordPlaceholder, placeholderAttributes);

            ForgotPasswordButton.SetTitle(Resources.LoginForgotPassword, UIControlState.Normal);

            var underscoreAttributes = new CTStringAttributes(new UIStringAttributes
            { 
                UnderlineColor = UIColor.White,
                ForegroundColor = UIColor.White,
                UnderlineStyle = NSUnderlineStyle.Single,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Medium)
            }.Dictionary);

            PrivacyPolicyButton.SetAttributedTitle(
                new NSAttributedString(Resources.PrivacyPolicy, underscoreAttributes), UIControlState.Normal);
            TermsOfServiceButton.SetAttributedTitle(
                new NSAttributedString(Resources.TermsOfService, underscoreAttributes), UIControlState.Normal);
        }

        private void prepareNavigationBar()
        {
            var attributes = new UITextAttributes { Font = UIFont.SystemFontOfSize(14, UIFontWeight.Medium) };
            var spaceFix = new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 8 };

            NavigationItem.LeftBarButtonItems = new[] { spaceFix, backButton };
            NavigationItem.RightBarButtonItems = new[] { spaceFix, nextButton };

            backButton.SetTitleTextAttributes(attributes, UIControlState.Normal);
            nextButton.SetTitleTextAttributes(attributes, UIControlState.Normal);
            backButton.SetTitleTextAttributes(attributes, UIControlState.Focused);
            nextButton.SetTitleTextAttributes(attributes, UIControlState.Focused);
            backButton.SetTitleTextAttributes(attributes, UIControlState.Disabled);
            nextButton.SetTitleTextAttributes(attributes, UIControlState.Disabled);
            backButton.SetTitleTextAttributes(attributes, UIControlState.Highlighted);
            nextButton.SetTitleTextAttributes(attributes, UIControlState.Highlighted);
        }

        private void startAnimations()
        {
            ActivityIndicator.StartAnimation();
        }
    }
}


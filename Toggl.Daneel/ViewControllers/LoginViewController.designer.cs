// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
	[Register ("LoginView")]
	partial class LoginViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		Toggl.Daneel.Views.ActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.NSLayoutConstraint BottomConstraint { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField EmailTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton ForgotPasswordButton { get; set; }

		[Outlet]
		UIKit.UIButton GoogleSignInButton { get; set; }

		[Outlet]
		UIKit.UIImageView GoogleSignInImage { get; set; }

		[Outlet]
		UIKit.UILabel InfoLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIImageView PasswordManagerButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextField PasswordTextField { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton PrivacyPolicyButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIImageView ShowPasswordButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIView SignUpLabels { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIButton TermsOfServiceButton { get; set; }

		[Outlet]
		UIKit.UIButton TryLoggingInInsteadButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (BottomConstraint != null) {
				BottomConstraint.Dispose ();
				BottomConstraint = null;
			}

			if (GoogleSignInImage != null) {
				GoogleSignInImage.Dispose ();
				GoogleSignInImage = null;
			}

			if (EmailTextField != null) {
				EmailTextField.Dispose ();
				EmailTextField = null;
			}

			if (ForgotPasswordButton != null) {
				ForgotPasswordButton.Dispose ();
				ForgotPasswordButton = null;
			}

			if (GoogleSignInButton != null) {
				GoogleSignInButton.Dispose ();
				GoogleSignInButton = null;
			}

			if (InfoLabel != null) {
				InfoLabel.Dispose ();
				InfoLabel = null;
			}

			if (TryLoggingInInsteadButton != null) {
				TryLoggingInInsteadButton.Dispose ();
				TryLoggingInInsteadButton = null;
			}

			if (PasswordManagerButton != null) {
				PasswordManagerButton.Dispose ();
				PasswordManagerButton = null;
			}

			if (PasswordTextField != null) {
				PasswordTextField.Dispose ();
				PasswordTextField = null;
			}

			if (PrivacyPolicyButton != null) {
				PrivacyPolicyButton.Dispose ();
				PrivacyPolicyButton = null;
			}

			if (ShowPasswordButton != null) {
				ShowPasswordButton.Dispose ();
				ShowPasswordButton = null;
			}

			if (SignUpLabels != null) {
				SignUpLabels.Dispose ();
				SignUpLabels = null;
			}

			if (TermsOfServiceButton != null) {
				TermsOfServiceButton.Dispose ();
				TermsOfServiceButton = null;
			}
		}
	}
}

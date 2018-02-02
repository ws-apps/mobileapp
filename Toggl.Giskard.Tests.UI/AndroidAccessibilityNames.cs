using System;
using Xamarin.UITest.Queries;

namespace Toggl.Tests.UI
{
    public static class Onboarding
    {
        public const string FirstOnboardingElement = LoginButton;
        public const string SkipButton = "";
        public const string NextButton = "";
        public const string LoginButton = "LOG IN";
        public const string SignUpButton = "NEW TO TOGGL";
        public const string FirstLabel = "";
        public const string SecondLabel = "";
        public const string ThirdLabel = "";
        public const string PreviousButton = "";
    }

    public static class Login
    {
        public const string EmailText = "LoginEmailTextField";
        public const string ErrorLabel = "LoginErrorTextField";
        public const string PasswordText = "LoginPasswordTextField";
        public const string ShowPasswordButton = "LoginShowPassword";
        public const string ForgotPasswordButton = "LoginForgotPassword";
        public const string BackButton = "Back Button";
        public const string NextButton = "Next Button";
    }

    public static class Main
    {
        public const string StartTimeEntryButton = "MainPlayButton";
        public const string StopTimeEntryButton = "MainStopButton";
    }

    public static class StartTimeEntry
    {
        public const string DoneButton = "StartTimeEntryDoneButton";
        public const string DescriptionText = "StartTimeEntryDescriptionTextField";
    }
}

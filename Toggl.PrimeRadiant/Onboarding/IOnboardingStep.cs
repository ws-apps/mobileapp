namespace Toggl.PrimeRadiant.Onboarding
{
    public interface IOnboardingStep
    {
        bool ShouldBeVisible { get; }
    }
}

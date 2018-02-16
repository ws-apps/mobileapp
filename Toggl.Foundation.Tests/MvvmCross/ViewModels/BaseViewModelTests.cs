using MvvmCross.Core.ViewModels;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public abstract class BaseViewModelTests<TViewModel> : BaseMvvmCrossTests
        where TViewModel : MvxViewModel
    {
        protected ITogglApi Api { get; } = Substitute.For<ITogglApi>();
        protected UserAgent UserAgent { get; } = new UserAgent("Foundation.Tests", "1.0");
        protected IMailService MailService { get; } = Substitute.For<IMailService>();
        protected IDialogService DialogService { get; } = Substitute.For<IDialogService>();
        protected ITimeService TimeService { get; } = Substitute.For<ITimeService>();
        protected ITogglDatabase Database { get; } = Substitute.For<ITogglDatabase>();
        protected ITogglDataSource DataSource { get; } = Substitute.For<ITogglDataSource>();
        protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();
        protected IPlatformConstants PlatformConstants { get; } = Substitute.For<IPlatformConstants>();

        protected IOnboardingStorage OnboardingStorage { get; } = Substitute.For<IOnboardingStorage>();

        protected TViewModel ViewModel { get; private set; }

        protected BaseViewModelTests()
        {
            Setup();
        }

        protected abstract TViewModel CreateViewModel();

        private void Setup()
        {
            AdditionalSetup();

            ViewModel = CreateViewModel();
        }

        protected virtual void AdditionalSetup()
        {
        }
    }
}
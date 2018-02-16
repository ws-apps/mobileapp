using System;
using FluentAssertions;
using MvvmCross.Core.Navigation;
using NSubstitute;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross
{
    public class FoundationExtensionsTests
    {
        public class TheRegisterServicesMethod : BaseMvvmCrossTests
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(FiveParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheParametersIsNull(
                bool useFoundation,
                bool useDialogService,
                bool useBrowserService,
                bool useKeyValueStorage,
                bool useNavigationService)
            {
                var foundation = useFoundation ? new Foundation() : null;
                var dialogService = useDialogService ? Substitute.For<IDialogService>() : null;
                var browserService = useBrowserService ? Substitute.For<IBrowserService>() : null;
                var keyValueStorage = useKeyValueStorage ? Substitute.For<IKeyValueStorage>() : null;
                var navigationService = useNavigationService ? Substitute.For<IMvxNavigationService>() : null;


                Action tryingToConstructWithEmptyParameters =
                    () => foundation.RegisterServices(
                            dialogService,
                            browserService,
                            keyValueStorage,
                            navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public class TheRevokeNewUserIfNeededMethod : BaseMvvmCrossTests
        {
            [Fact, LogIfTooSlow]
            public void MarksTheUserAsNotNewWhenUsingTheAppForTheFirstTimeAfterSixtyDays()
            {
                var version = "1.0";
                var now = DateTimeOffset.Now;
                var apiFactory = Substitute.For<IApiFactory>();
                var database = Substitute.For<ITogglDatabase>();
                var timeService = Substitute.For<ITimeService>();
                var googleService = Substitute.For<IGoogleService>();
                var backgroundService = Substitute.For<IBackgroundService>();
                var keyValueStorage = Substitute.For<IKeyValueStorage>();
                var apiErrorHandlingService = Substitute.For<IApiErrorHandlingService>();
                var applicationShortcutCreator = Substitute.For<IApplicationShortcutCreator>();
                var settingsService = new SettingsStorage(Version.Parse(version), keyValueStorage);
                var foundationMvvmCross = new FoundationMvvmCross(
                    apiFactory,
                    database,
                    timeService,
                    googleService,
                    applicationShortcutCreator,
                    backgroundService,
                    settingsService,
                    NavigationService,
                    apiErrorHandlingService);
                timeService.CurrentDateTime.Returns(now);
                keyValueStorage.GetString("LastAccessDate").Returns(now.AddDays(-60).ToString());

                foundationMvvmCross.RevokeNewUserIfNeeded();

                keyValueStorage.Received().SetBool("IsNewUser", false);
            }
        }
    }
}

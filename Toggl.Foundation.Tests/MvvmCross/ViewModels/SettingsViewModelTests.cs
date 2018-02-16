using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Toggl.PrimeRadiant.Settings;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SettingsViewModelTests
    {
        public abstract class SettingsViewModelTest : BaseViewModelTests<SettingsViewModel>
        {
            protected ISubject<SyncProgress> ProgressSubject;
            protected IUserPreferences UserPreferences;

            protected override void AdditionalSetup()
            {
                ProgressSubject = new Subject<SyncProgress>();
                var syncManager = Substitute.For<ISyncManager>();
                syncManager.ProgressObservable.Returns(ProgressSubject.AsObservable());
                DataSource.SyncManager.Returns(syncManager);
                UserPreferences = Substitute.For<IUserPreferences>();
            }

            protected override SettingsViewModel CreateViewModel()
                => new SettingsViewModel(
                    UserAgent,
                    MailService,
                    DataSource,
                    DialogService,
                    PlatformConstants,
                    UserPreferences,
                    NavigationService);
        }

        public sealed class TheConstructor : SettingsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(SevenParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useUserAgent,
                bool useDataSource,
                bool useMailService,
                bool useDialogService,
                bool usePlatformConstants,
                bool useUserPreferences,
                bool useNavigationService)
            {
                var userAgent = useUserAgent ? UserAgent : null;
                var dataSource = useDataSource ? DataSource : null;
                var mailService = useMailService ? MailService : null;
                var dialogService = useDialogService ? DialogService : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var platformConstants = usePlatformConstants ? PlatformConstants : null;
                var userPreferences = useUserPreferences ? UserPreferences : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SettingsViewModel(
                        userAgent,
                        mailService,
                        dataSource,
                        dialogService,
                        platformConstants,
                        userPreferences,
                        navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheFlags : SettingsViewModelTest
        {
            [Property]
            public void SetsIsRunningSyncCorrectly(NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsRunningSync.Should().Be(state == SyncProgress.Syncing);
                }
            }

            [Property]
            public void SetsIsSyncedCorrectly(NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    if (state == SyncProgress.Unknown)
                        continue;

                    ProgressSubject.OnNext(state);
                    ViewModel.IsSynced.Should().Be(state == SyncProgress.Synced);
                }
            }

            [Property]
            public void DoesNotEverSetBothIsRunningSyncAndIsSyncedBothToTrue(NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    (ViewModel.IsRunningSync && ViewModel.IsSynced).Should().BeFalse();
                }
            }

            [Property]
            public void DoesNotSetTheIsLoggingOutFlagIfTheLogoutCommandIsNotExecuted(
                NonEmptyArray<SyncProgress> statuses)
            {
                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsLoggingOut.Should().BeFalse();
                }
            }

            [Property]
            public void DoesNotUnsetTheIsLoggingOutFlagAfterItIsSetNoMatterWhatStatusesAreObserved(
                NonEmptyArray<SyncProgress> statuses)
            {
                DataSource.Logout().Returns(Observable.Never<Unit>());

                ViewModel.LogoutCommand.ExecuteAsync();

                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsLoggingOut.Should().BeTrue();
                }
            }

            [Property]
            public void SetsTheIsRunningSyncAndIsSyncedFlagsToFalseAfterTheIsLoggingInFlagIsSetAndDoesNotSetThemToTrueNoMatterWhatStatusesAreObserved(NonEmptyArray<SyncProgress> statuses)
            {
                DataSource.Logout().Returns(Observable.Never<Unit>());

                ViewModel.LogoutCommand.ExecuteAsync();

                foreach (var state in statuses.Get)
                {
                    ProgressSubject.OnNext(state);
                    ViewModel.IsRunningSync.Should().BeFalse();
                    ViewModel.IsSynced.Should().BeFalse();
                }
            }
        }

        public sealed class TheLogoutCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task SetsTheIsLoggingOutFlagToTrue()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                ViewModel.IsLoggingOut.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public async Task CallsLogoutOnTheDataSource()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                await DataSource.Received().Logout();
            }

            [Fact, LogIfTooSlow]
            public async Task ResetsUserPreferences()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                UserPreferences.Received().Reset();
            }

            [Fact, LogIfTooSlow]
            public async Task NavigatesToTheOnboardingScreen()
            {
                doNotShowConfirmationDialog();
                await ViewModel.LogoutCommand.ExecuteAsync();

                await NavigationService.Received().Navigate<OnboardingViewModel>();
            }

            [Fact, LogIfTooSlow]
            public void ChecksIfThereAreUnsyncedDataWhenTheSyncProcessFinishes()
            {
                ProgressSubject.OnNext(SyncProgress.Synced);

                DataSource.Received().HasUnsyncedData();
            }

            [Fact, LogIfTooSlow]
            public void SetsTheIsSyncedFlagAfterTheSyncProcessHasFinishedAndThereIsNoTimeEntryToPush()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Synced);

                ViewModel.IsSynced.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void UnsetsTheIsSyncedFlagWhenTheSyncProcessIsNotRunningButThrereIsSomeTimeEntryToPush()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(true));
                ProgressSubject.OnNext(SyncProgress.Synced);

                ViewModel.IsSynced.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void UnsetsTheIsSyncedFlagWhenThereIsNothingToPushButTheSyncProcessStartsAgain()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Synced);
                ProgressSubject.OnNext(SyncProgress.Syncing);

                ViewModel.IsSynced.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotShowConfirmationDialogWhenTheAppIsInSync()
            {
                doNotShowConfirmationDialog();

                await ViewModel.LogoutCommand.ExecuteAsync();

                await DialogService.DidNotReceiveWithAnyArgs().Confirm("", "", "", "");
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsConfirmationDialogWhenThereIsNothingToPushButSyncIsRunning()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Syncing);

                await ViewModel.LogoutCommand.ExecuteAsync();

                await DialogService.ReceivedWithAnyArgs().Confirm("", "", "", "");
            }

            [Fact, LogIfTooSlow]
            public async Task ShowsConfirmationDialogWhenThereIsSomethingToPushButSyncIsNotRunning()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(true));
                ProgressSubject.OnNext(SyncProgress.Syncing);

                await ViewModel.LogoutCommand.ExecuteAsync();

                await DialogService.ReceivedWithAnyArgs().Confirm("", "", "", "");
            }

            [Fact, LogIfTooSlow]
            public async Task DoesNotProceedWithLogoutWhenUserClicksCancelButtonInTheDialog()
            {
                ProgressSubject.OnNext(SyncProgress.Syncing);
                DialogService.Confirm(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>()).Returns(false);

                await ViewModel.LogoutCommand.ExecuteAsync();

                ViewModel.IsLoggingOut.Should().BeFalse();
                await DataSource.DidNotReceive().Logout();
                await NavigationService.DidNotReceive().Navigate<OnboardingViewModel>();
            }

            [Fact, LogIfTooSlow]
            public async Task ProceedsWithLogoutWhenUserClicksSignOutButtonInTheDialog()
            {
                ProgressSubject.OnNext(SyncProgress.Syncing);
                DialogService.Confirm(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>()).Returns(true);

                await ViewModel.LogoutCommand.ExecuteAsync();

                ViewModel.IsLoggingOut.Should().BeTrue();
                await DataSource.Received().Logout();
                await NavigationService.Received().Navigate<OnboardingViewModel>();
            }

            private void doNotShowConfirmationDialog()
            {
                DataSource.HasUnsyncedData().Returns(Observable.Return(false));
                ProgressSubject.OnNext(SyncProgress.Synced);
            }
        }

        public sealed class ThePickWorkspaceCommand : SettingsViewModelTest
        {
            private const long workspaceId = 10;
            private const long defaultWorkspaceId = 11;
            private const string workspaceName = "My custom workspace";
            private readonly IDatabaseWorkspace workspace = Substitute.For<IDatabaseWorkspace>();
            private readonly IDatabaseWorkspace defaultWorkspace = Substitute.For<IDatabaseWorkspace>();

            public ThePickWorkspaceCommand()
            {
                workspace.Id.Returns(workspaceId);
                workspace.Name.Returns(workspaceName);
                defaultWorkspace.Id.Returns(defaultWorkspaceId);

                DataSource.Workspaces.GetDefault()
                    .Returns(Observable.Return(defaultWorkspace));

                DataSource.Workspaces.GetById(workspaceId)
                    .Returns(Observable.Return(workspace));

                ViewModel.Prepare();
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheSelectWorkspaceViewModel()
            {
                await ViewModel.EditWorkspaceCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>());
            }

            [Fact, LogIfTooSlow]
            public async Task SetsTheReturnedWorkspaceNameAsTheWorkspaceNameProperty()
            {
                NavigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>())
                    .Returns(Task.FromResult(workspaceId));

                await ViewModel.EditWorkspaceCommand.ExecuteAsync();

                ViewModel.WorkspaceName.Should().Be(workspaceName);
            }

            [Fact, LogIfTooSlow]
            public async Task UpdatesTheUserWithTheReceivedWorspace()
            {
                NavigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>())
                    .Returns(Task.FromResult(workspaceId));

                await ViewModel.EditWorkspaceCommand.ExecuteAsync();

                await DataSource.User.Received().UpdateWorkspace(Arg.Is(workspaceId));
            }

            [Fact, LogIfTooSlow]
            public async Task StartsTheSyncAlgorithm()
            {
                NavigationService
                    .Navigate<SelectWorkspaceViewModel, WorkspaceParameters, long>(Arg.Any<WorkspaceParameters>())
                    .Returns(Task.FromResult(workspaceId));

                await ViewModel.EditWorkspaceCommand.ExecuteAsync();

                await DataSource.SyncManager.Received().PushSync();
            }
        }

        public sealed class TheBackCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.BackCommand.ExecuteAsync();

                await NavigationService.Received().Close(ViewModel);
            }
        }

        public sealed class TheToggleAddMobileTagCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void TogglesTheCurrentValueOfTheToggleAddMobileTagProperty()
            {
                var expected = !ViewModel.AddMobileTag;

                ViewModel.ToggleAddMobileTagCommand.Execute();

                ViewModel.AddMobileTag.Should().Be(expected);
            }
        }

        public sealed class TheToggleManualModeCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ChangesManualModeToTimerMode()
            {
                UserPreferences.IsManualModeEnabled().Returns(true);

                await ViewModel.Initialize();
                ViewModel.ToggleManualModeCommand.Execute();

                ViewModel.IsManualModeEnabled.Should().BeFalse();
                UserPreferences.Received().EnableTimerMode();
            }

            [Fact, LogIfTooSlow]
            public async Task ChangesTimerModeToManualMode()
            {
                UserPreferences.IsManualModeEnabled().Returns(true);

                await ViewModel.Initialize();
                ViewModel.ToggleManualModeCommand.Execute();

                ViewModel.IsManualModeEnabled.Should().BeFalse();
                UserPreferences.Received().EnableTimerMode();
            }
        }


        public sealed class TheToggleUseTwentyFourHourClockCommand : SettingsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void TogglesTheCurrentValueOfTheToggleUseTwentyFourHourClockProperty()
            {
                var expected = !ViewModel.UseTwentyFourHourClock;

                ViewModel.ToggleUseTwentyFourHourClockCommand.Execute();

                ViewModel.UseTwentyFourHourClock.Should().Be(expected);
            }
        }

        public sealed class TheHelpCommand : SettingsViewModelTest
        {
            [Property]
            public void NavigatesToBrowserViewModelWithUrlFromPlatformConstants(
                NonEmptyString nonEmptyString)
            {
                var helpUrl = nonEmptyString.Get;
                PlatformConstants.HelpUrl.Returns(helpUrl);

                ViewModel.HelpCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Url == helpUrl));
            }

            [Fact, LogIfTooSlow]
            public void NavigatesToBrowserViewModelWithHelpTitle()
            {
                ViewModel.HelpCommand.Execute();

                NavigationService.Received().Navigate<BrowserViewModel, BrowserParameters>(
                    Arg.Is<BrowserParameters>(parameter => parameter.Title == Resources.Help));
            }
        }

        public sealed class TheSubmitFeedbackCommand : SettingsViewModelTest
        {
            [Property]
            public void SendsAnEmailToTogglSupport(
                NonEmptyString nonEmptyString0, NonEmptyString nonEmptyString1)
            {
                var phoneModel = nonEmptyString0.Get;
                var os = nonEmptyString1.Get;
                PlatformConstants.PhoneModel.Returns(phoneModel);
                PlatformConstants.OperatingSystem.Returns(os);

                ViewModel.SubmitFeedbackCommand.Execute();

                MailService
                    .Received()
                    .Send(
                        "support@toggl.com",
                        Arg.Any<string>(),
                        Arg.Any<string>())
                    .Wait();
            }

            [Property]
            public void SendsAnEmailWithTheProperSubject(
                NonEmptyString nonEmptyString)
            {
                var subject = nonEmptyString.Get;
                PlatformConstants.FeedbackEmailSubject.Returns(subject);

                ViewModel.SubmitFeedbackCommand.ExecuteAsync().Wait();

                MailService.Received()
                    .Send(
                        Arg.Any<string>(),
                        subject,
                        Arg.Any<string>())
                   .Wait();
            }

            [Fact, LogIfTooSlow]
            public async Task SendsAnEmailWithAppVersionPhoneModelAndOsVersion()
            {
                PlatformConstants.PhoneModel.Returns("iPhone Y");
                PlatformConstants.OperatingSystem.Returns("iOS 4.2.0");
                var expectedMessage = $"\n\nVersion: {UserAgent.ToString()}\nPhone: {PlatformConstants.PhoneModel}\nOS: {PlatformConstants.OperatingSystem}";

                await ViewModel.SubmitFeedbackCommand.ExecuteAsync();

                await MailService.Received().Send(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    expectedMessage);
            }

            [Property]
            public void AlertsUserWhenMailServiceReturnsAnError(
                NonEmptyString nonEmptyString0, NonEmptyString nonEmptyString1)
            {
                var errorTitle = nonEmptyString0.Get;
                var errorMessage = nonEmptyString1.Get;
                var result = new MailResult(false, errorTitle, errorMessage);
                MailService
                    .Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                    .Returns(Task.FromResult(result));

                ViewModel.SubmitFeedbackCommand.Execute();

                DialogService
                    .Received()
                    .Alert(errorTitle, errorMessage, Resources.Ok)
                    .Wait();
            }

            [Theory, LogIfTooSlow]
            [InlineData(true, "")]
            [InlineData(true, "Error")]
            [InlineData(true, null)]
            [InlineData(false, "")]
            [InlineData(false, null)]
            public async Task DoesNotAlertUserWhenMailServiceReturnsSuccessOrDoesNotHaveErrorTitle(
                bool success, string errorTitle)
            {
                var result = new MailResult(success, errorTitle, "");
                MailService
                    .Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                    .Returns(Task.FromResult(result));

                await ViewModel.SubmitFeedbackCommand.ExecuteAsync();

                await DialogService
                    .DidNotReceive()
                    .Alert(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            }
        }
    }
}

﻿using System;
using System.Reactive.Concurrency;
using MvvmCross.Core.Navigation;
using MvvmCross.Platform;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using Toggl.Foundation.Sync;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;

namespace Toggl.Foundation.MvvmCross
{
    public sealed class FoundationMvvmCross
    {
        internal ITogglDatabase Database { get; }

        internal IApiFactory ApiFactory { get; }

        internal ITimeService TimeService { get; }

        internal IAnalyticsService AnalyticsService { get; }

        internal IGoogleService GoogleService { get; }

        internal SettingsStorage SettingsStorage { get; }

        internal IMvxNavigationService NavigationService { get; }

        internal IApiErrorHandlingService ApiErrorHandlingService { get; }

        internal IBackgroundService BackgroundService { get; }

        internal IApplicationShortcutCreator ShortcutCreator { get; }

        internal FoundationMvvmCross(
            IApiFactory apiFactory,
            ITogglDatabase database,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            IGoogleService googleService,
            IApplicationShortcutCreator shortcutCreator,
            IBackgroundService backgroundService,
            SettingsStorage settingsStorage,
            IMvxNavigationService navigationService,
            IApiErrorHandlingService apiErrorHandlingService)
        {
            Database = database;
            ApiFactory = apiFactory;
            TimeService = timeService;
            AnalyticsService = analyticsService;
            GoogleService = googleService;
            ShortcutCreator = shortcutCreator;
            BackgroundService = backgroundService;
            SettingsStorage = settingsStorage;
            NavigationService = navigationService;
            ApiErrorHandlingService = apiErrorHandlingService;
        }
    }

    public static class FoundationExtensions
    {
        private const int newUserThreshold = 60;
        private static readonly TimeSpan retryDelayLimit = TimeSpan.FromSeconds(60);

        public static FoundationMvvmCross RegisterServices(this Foundation self,
            IDialogService dialogService,
            IBrowserService browserService,
            IKeyValueStorage keyValueStorage,
            IMvxNavigationService navigationService,
            IPasswordManagerService passwordManagerService = null)
        {
            Ensure.Argument.IsNotNull(self, nameof(self));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(browserService, nameof(browserService));
            Ensure.Argument.IsNotNull(keyValueStorage, nameof(keyValueStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            var timeService = self.TimeService;

            var settingsStorage = new SettingsStorage(self.Version, keyValueStorage);
            var apiErrorHandlingService = new ApiErrorHandlingService(navigationService, settingsStorage);

            Mvx.RegisterSingleton(self.BackgroundService);
            Mvx.RegisterSingleton(dialogService);
            Mvx.RegisterSingleton(browserService);
            Mvx.RegisterSingleton(self.UserAgent);
            Mvx.RegisterSingleton(self.TimeService);
            Mvx.RegisterSingleton(self.ShortcutCreator);
            Mvx.RegisterSingleton(self.MailService);
            Mvx.RegisterSingleton(self.AnalyticsService);
            Mvx.RegisterSingleton(self.PlatformConstants);
            Mvx.RegisterSingleton(self.SuggestionProviderContainer);
            Mvx.RegisterSingleton<IOnboardingStorage>(settingsStorage);
            Mvx.RegisterSingleton<IAccessRestrictionStorage>(settingsStorage);
            Mvx.RegisterSingleton<IUserPreferences>(settingsStorage);
            Mvx.RegisterSingleton<IApiErrorHandlingService>(apiErrorHandlingService);
            Mvx.RegisterSingleton(passwordManagerService ?? new StubPasswordManagerService());

            return new FoundationMvvmCross(
                self.ApiFactory,
                self.Database,
                timeService,
                self.AnalyticsService,
                self.GoogleService,
                self.ShortcutCreator,
                self.BackgroundService,
                settingsStorage,
                navigationService,
                apiErrorHandlingService);
        }

        public static FoundationMvvmCross RevokeNewUserIfNeeded(this FoundationMvvmCross self)
        {
            var now = self.TimeService.CurrentDateTime;
            var lastUsed = self.SettingsStorage.GetLastOpened();
            self.SettingsStorage.SetLastOpened(now);
            if (lastUsed == null) return self;

            var lastUsedDate = DateTimeOffset.Parse(lastUsed);
            var offset = now - lastUsedDate;
            if (offset < TimeSpan.FromDays(newUserThreshold)) return self;

            self.SettingsStorage.SetIsNewUser(false);
            return self;
        }

        public static void Initialize(this FoundationMvvmCross self, App app, IScheduler scheduler)
        {
            Func<ITogglDataSource, ISyncManager> createSyncManager(ITogglApi api) => dataSource =>
                TogglSyncManager.CreateSyncManager(self.Database, api, dataSource, self.TimeService, self.AnalyticsService, retryDelayLimit, scheduler);

            ITogglDataSource createDataSource(ITogglApi api)
            => new TogglDataSource(api, self.Database, self.TimeService, self.ApiErrorHandlingService, self.BackgroundService, createSyncManager(api), TimeSpan.FromMinutes(5), self.ShortcutCreator)
                    .RegisterServices();

            var loginManager =
                new LoginManager(self.ApiFactory, self.Database, self.GoogleService, self.ShortcutCreator, self.SettingsStorage, createDataSource);

            app.Initialize(loginManager, self.NavigationService, self.SettingsStorage);
        }
    }
}

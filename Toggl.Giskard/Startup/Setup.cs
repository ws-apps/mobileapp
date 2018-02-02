using System.Reactive.Concurrency;
using Android.Content;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Views;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform.Plugins;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.Suggestions;
using Toggl.Giskard.Presenters;
using Toggl.Giskard.Services;
using Toggl.PrimeRadiant.Realm;
using Toggl.Ultrawave;

namespace Toggl.Giskard
{
    public sealed partial class Setup : MvxAppCompatSetup
    {
        private const int maxNumberOfSuggestions = 5;

        private IMvxNavigationService navigationService;

#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        public Setup(Context applicationContext) 
            : base(applicationContext)
        {
        }

        protected override IMvxTrace CreateDebugTrace() => new DebugTrace();

        protected override IMvxApplication CreateApp() => new App();

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
            => navigationService = base.InitializeNavigationService(collection);

        protected override IMvxAndroidViewPresenter CreateViewPresenter() 
            => new TogglPresenter(AndroidViewAssemblies);

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            base.InitializeApp(pluginManager, app);

            const string clientName = "Giskard";
            var packageInfo = ApplicationContext.PackageManager.GetPackageInfo(ApplicationContext.PackageName, 0);
            var version = packageInfo.VersionName;
            var sharedPreferences = ApplicationContext.GetSharedPreferences(clientName, FileCreationMode.Private);
            var database = new Database();
            var timeService = new TimeService(Scheduler.Default);
            var suggestionProviderContainer = new SuggestionProviderContainer(
                new MostUsedTimeEntrySuggestionProvider(database, timeService, maxNumberOfSuggestions)
            );

            var foundation = Foundation.Foundation.Create(
                clientName,
                version,
                database,
                timeService,
                new MailService(),
                new GoogleService(),
                environment,
                new AnalyticsService(),
                new PlatformConstants(),
                new ApplicationShortcutCreator(suggestionProviderContainer),
                suggestionProviderContainer
            );

            foundation
                .RegisterServices(
                    new DialogService(),
                    new BrowserService(), 
                    new SharedPreferencesStorage(sharedPreferences),
                    navigationService,
                    new OnePasswordService())
               .RevokeNewUserIfNeeded()
               .Initialize(app as App, Scheduler.Default);
        }
    }
}

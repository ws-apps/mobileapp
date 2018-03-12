using System.Reactive.Concurrency;
using Foundation;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Platform;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform.Plugins;
using Toggl.Daneel.Presentation;
using Toggl.Daneel.Services;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant.Realm;
using Toggl.Ultrawave;
using UIKit;

namespace Toggl.Daneel
{
    public partial class Setup : MvxIosSetup
    {
        private const int maxNumberOfSuggestions = 3;

        private IAnalyticsService analyticsService;
        private IMvxNavigationService navigationService;

#if USE_PRODUCTION_API
        private const ApiEnvironment environment = ApiEnvironment.Production;
#else
        private const ApiEnvironment environment = ApiEnvironment.Staging;
#endif

        public Setup(MvxApplicationDelegate applicationDelegate, UIWindow window)
            : this(applicationDelegate, new TogglPresenter(applicationDelegate, window))
        {
        }

        private Setup(MvxApplicationDelegate applicationDelegate, IMvxIosViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }

        protected override IMvxTrace CreateDebugTrace() => new DebugTrace();

        protected override IMvxApplication CreateApp() => new App();

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
        {
            analyticsService = new AnalyticsService();

            var loader = CreateViewModelLoader(collection);
            Mvx.RegisterSingleton<IMvxViewModelLoader>(loader);

            navigationService = new TrackingNavigationService(null, loader, analyticsService);

            Mvx.RegisterSingleton<IMvxNavigationService>(navigationService);
            return navigationService;
        }

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            base.InitializeApp(pluginManager, app);

#if !USE_PRODUCTION_API
            System.Net.ServicePointManager.ServerCertificateValidationCallback
                  += (sender, certificate, chain, sslPolicyErrors) => true;
#endif

            const string clientName = "Daneel";
            var version = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
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
                new MailService((ITopViewControllerProvider)Presenter),
                new GoogleService(),
                environment,
                analyticsService,
                new PlatformConstants(),
                new ApplicationShortcutCreator(suggestionProviderContainer),
                suggestionProviderContainer
            );

            foundation
                .RegisterServices(
                    new DialogService((ITopViewControllerProvider)Presenter),
                    new BrowserService(),
                    new UserDefaultsStorage(),
                    navigationService,
                    new OnePasswordService())
                .RevokeNewUserIfNeeded()
                .Initialize(app as App, Scheduler.Default);
        }
    }
}

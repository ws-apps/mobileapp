using MvvmCross.Core;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.Platform;
using MvvmCross.Platform.Core;
using MvvmCross.Platform.IoC;
using MvvmCross.Platform.Platform;
using MvvmCross.Test.Core;
using NSubstitute;

namespace Toggl.Foundation.Tests.MvvmCross
{
    public abstract class BaseMvvmCrossTests : InteractorAwareTests
    {
        protected IMvxIoCProvider Ioc { get; private set; }

        protected IMvxNavigationService NavigationService { get; } = Substitute.For<IMvxNavigationService>();

        static BaseMvvmCrossTests()
        {
            if (MvxSingleton<IMvxIoCProvider>.Instance != null) return;

            MvxSingletonCache.Initialize();
            MvxSimpleIoCContainer.Initialize();

            MvxSingleton<IMvxIoCProvider>.Instance.RegisterSingleton<IMvxTrace>(new TestTrace());
            MvxSingleton<IMvxIoCProvider>.Instance.RegisterSingleton<IMvxSettings>(new MvxSettings());
            
            MvxTrace.Initialize();
        }

        protected BaseMvvmCrossTests()
        {
            Ioc = MvxSingleton<IMvxIoCProvider>.Instance;
        }
    }
}

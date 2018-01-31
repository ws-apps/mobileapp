using System;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Login;
using Toggl.Foundation.Services;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Xunit;

namespace Toggl.Foundation.Tests
{
    public sealed class FoundationTests
    {
        public class TheCreateMethod
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(EightParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useClientName,
                bool useVersion,
                bool useDatabase,
                bool useTimeService,
                bool useMailService,
                bool useGoogleService,
                bool useAnalyticsService,
                bool usePlatformConstants)
            {
                var version = useVersion ? "1.0" : null;
                var clientName = useClientName ? "Some Client" : null;
                var database = useDatabase ? Substitute.For<ITogglDatabase>() : null;
                var timeService = useTimeService ? Substitute.For<ITimeService>() : null;
                var mailService = useMailService ? Substitute.For<IMailService>() : null;
                var googleService = useGoogleService ? Substitute.For<IGoogleService>() : null;
                var analyticsService = useAnalyticsService ? Substitute.For<IAnalyticsService>() : null;
                var platformConstants = usePlatformConstants ? Substitute.For<IPlatformConstants>() : null;

                Action tryingToConstructWithEmptyParameters =
                    () => Foundation.Create(
                        clientName,
                        version,
                        database,
                        timeService,
                        mailService,
                        googleService,
                        ApiEnvironment.Staging,
                        analyticsService,
                        platformConstants
                    );

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }
    }
}

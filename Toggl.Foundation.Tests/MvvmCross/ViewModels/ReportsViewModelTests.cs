using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Threading;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Reports;
using Toggl.Foundation.Tests.Generators;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class ReportsViewModelTests
    {
        public abstract class ReportsViewModelTest : BaseViewModelTests<ReportsViewModel>
        {
            protected const long WorkspaceId = 10;

            protected IReportsProvider ReportsProvider { get; } = Substitute.For<IReportsProvider>();

            protected override ReportsViewModel CreateViewModel()
            {
                DataSource.ReportsProvider.Returns(ReportsProvider);
                return new ReportsViewModel(DataSource, TimeService, NavigationService);
            }
        }

        public sealed class TheConstructor : ReportsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource,
                                                        bool useTimeService,
                                                        bool useNavigationService)
            {
                var timeService = useTimeService ? TimeService : null;
                var reportsProvider = useDataSource ? DataSource : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new ReportsViewModel(reportsProvider, timeService, navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class ThePrepareMethod : ReportsViewModelTest
        {
            [Property(MaxTest = 1)]
            public void FiresACallToLoadReports(DateTimeOffset now)
            {
                var date = now.Date;
                TimeService.CurrentDateTime.Returns(now);
                var expectedStartDate = date.AddDays(1 - (int)date.DayOfWeek);
                ViewModel.Prepare(WorkspaceId);

                ReportsProvider.Received().GetProjectSummary(
                    WorkspaceId, expectedStartDate, expectedStartDate.AddDays(6));
            }
        }

        public sealed class TheBillablePercentageMethod : ReportsViewModelTest
        {
            [Property(MaxTest = 1)]
            public void IsSetToNullIfTheTotalTimeOfAReportIsZero(DateTimeOffset now)
            {
                var date = now.Date;
                TimeService.CurrentDateTime.Returns(now);
                var expectedStartDate = date.AddDays(1 - (int)date.DayOfWeek);
                ReportsProvider.GetProjectSummary(
                    WorkspaceId, expectedStartDate, expectedStartDate.AddDays(6))
                    .Returns(Observable.Return(new ProjectSummaryReport(new ChartSegment[0])));

                ViewModel.Prepare(WorkspaceId);

                ViewModel.BillablePercentage.Should().BeNull();
            }
        }

        public sealed class TheIsLoadingProperty : ReportsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsSetToTrueBeforeWhenAReportIsLoading()
            {
                var now = DateTimeOffset.Now;
                TimeService.CurrentDateTime.Returns(now);
                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Never<ProjectSummaryReport>());

                ViewModel.Prepare(WorkspaceId);

                ViewModel.IsLoading.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void IsSetToFalseWhenLoadingIsCompleted()
            {
                var now = DateTimeOffset.Now;
                TimeService.CurrentDateTime.Returns(now);
                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Return(new ProjectSummaryReport(new ChartSegment[0])));

                ViewModel.Prepare(WorkspaceId);

                ViewModel.IsLoading.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void IsSetToFalseWhenLoadingOverBecauseOfAnError()
            {
                var now = DateTimeOffset.Now;
                TimeService.CurrentDateTime.Returns(now);
                ReportsProvider.GetProjectSummary(Arg.Any<long>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>())
                    .Returns(Observable.Throw<ProjectSummaryReport>(new Exception()));

                ViewModel.Prepare(WorkspaceId);

                ViewModel.IsLoading.Should().BeFalse();
            }
        }

        public sealed class TheCurrentDateRangeStringProperty : ReportsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void IsInitializedToThisWeek()
            {
                TimeService.CurrentDateTime.Returns(new DateTimeOffset(2017, 10, 10, 10, 10, 10, TimeSpan.Zero));
                ViewModel.Prepare(WorkspaceId);

                ViewModel.CurrentDateRangeString.Should().Be($"{Resources.ThisWeek} ▾");
            }

            [Theory, LogIfTooSlow]
            [InlineData(
                2017, 12, 6,
                2017, 12, 4,
                2017, 12, 10
            )]
            [InlineData(
                2018, 2, 28,
                2018, 2, 26,
                2018, 3, 4
            )]
            [InlineData(
                2016, 4, 20,
                2016, 4, 18,
                2016, 4, 24
            )]
            [InlineData(
                2016, 12, 28,
                2016, 12, 26,
                2017, 1, 1
            )]
            public void ReturnsThisWeekWhenStartAndEndOfCurrentWeekAreSeleted(
                int currentYear, int currentMonth, int currentDay,
                int startYear, int startMonth, int startDay,
                int endYear, int endMonth, int endDay)
            {
                var currentDate = new DateTimeOffset(currentYear, currentMonth, currentDay, 0, 0, 0, TimeSpan.Zero);
                var start = new DateTimeOffset(startYear, startMonth, startDay, 0, 0, 0, TimeSpan.Zero);
                var end = new DateTimeOffset(endYear, endMonth, endDay, 0, 0, 0, TimeSpan.Zero);
                TimeService.CurrentDateTime.Returns(currentDate);
                ViewModel.ChangeDateRangeCommand.Execute(
                    DateRangeParameter.WithDates(start, end));

                ViewModel.CurrentDateRangeString.Should().Be($"{Resources.ThisWeek} ▾");
            }

            [Theory, LogIfTooSlow]
            [MemberData(nameof(DateRangeFormattingTestData))]
            public void ReturnsSelectedDateRangeAsStringIfTheSelectedPeriodIsNotTheCurrentWeek(
                int startYear, int startMonth, int startDay,
                int endYear, int endMonth, int endDay,
                CultureInfo deviceCulture,
                string expectedResult)
            {
                Thread.CurrentThread.CurrentCulture = deviceCulture;
                var start = new DateTimeOffset(startYear, startMonth, startDay, 10, 12, 13, TimeSpan.Zero);
                var end = new DateTimeOffset(endYear, endMonth, endDay, 12, 34, 1, TimeSpan.Zero);

                ViewModel.ChangeDateRangeCommand.Execute(
                    DateRangeParameter.WithDates(start, end));

                ViewModel.CurrentDateRangeString.Should().Be(expectedResult);
            }

            public static IEnumerable<object[]> DateRangeFormattingTestData()
            {
                var cultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("en-GB"),
                    new CultureInfo("de-DE"),
                    new CultureInfo("jp-JP"),
                    new CultureInfo("cs-CS"),
                    new CultureInfo("ru-RU")
                };

                foreach (var culture in cultures)
                {
                    yield return new object[]
                    {
                        2017, 12, 15,
                        2017, 12, 25,
                        culture,
                        "15 Dec - 25 Dec ▾"
                    };

                    yield return new object[]
                    {
                        2017, 1, 1,
                        2017, 12, 30,
                        culture,
                        "1 Jan - 30 Dec ▾"
                    };

                    yield return new object[]
                    {
                        2017, 11, 13,
                        2018, 11, 13,
                        culture,
                        "13 Nov - 13 Nov ▾"
                    };
                }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Reports;
using Toggl.Foundation.Tests.Generators;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;
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

            [Theory]
            [MemberData(nameof(DateRangeFormattingTestData))]
            public void ReturnsSelectedDateRangeAsStringIfTheSelectedPeriodIsNotTheCurrentWeek(
                DateTimeOffset start,
                DateTimeOffset end,
                DateFormat dateFormat,
                string expectedResult)
            {
                TimeService.CurrentDateTime.Returns(DateTimeOffset.UtcNow);
                var preferences = Substitute.For<IDatabasePreferences>();
                preferences.DateFormat.Returns(dateFormat);
                var preferencesSubject = new Subject<IDatabasePreferences>();
                DataSource.Preferences.Current.Returns(preferencesSubject.AsObservable());
                ViewModel.Prepare(0);
                preferencesSubject.OnNext(preferences);

                ViewModel.ChangeDateRangeCommand.Execute(
                    DateRangeParameter.WithDates(start, end));

                ViewModel.CurrentDateRangeString.Should().Be(expectedResult);
            }

            public static IEnumerable<object[]> DateRangeFormattingTestData()
            {
                var dateFormats = new[]
                {
                    DateFormat.FromLocalizedDateFormat("YYYY-MM-DD"),
                    DateFormat.FromLocalizedDateFormat("DD.MM.YYYY"),
                    DateFormat.FromLocalizedDateFormat("DD-MM-YYYY"),
                    DateFormat.FromLocalizedDateFormat("MM/DD/YYYY"),
                    DateFormat.FromLocalizedDateFormat("DD/MM/YYYY"),
                    DateFormat.FromLocalizedDateFormat("MM-DD-YYYY")
                };

                var ranges = new[]
                {
                    (new DateTimeOffset(2017, 12, 15, 10, 12, 13, TimeSpan.Zero), new DateTimeOffset(2017, 12, 15, 12, 34, 1, TimeSpan.Zero)),
                    (new DateTimeOffset(2017, 1, 1, 10, 12, 13, TimeSpan.Zero), new DateTimeOffset(2017, 12, 30, 12, 34, 1, TimeSpan.Zero)),
                    (new DateTimeOffset(2017, 11, 13, 10, 12, 13, TimeSpan.Zero), new DateTimeOffset(2018, 11, 13, 12, 34, 1, TimeSpan.Zero))
                };

                string expectedTitleString(DateTimeOffset start, DateTimeOffset end, DateFormat dateFormat)
                    => $"{start.ToString(dateFormat.Short)} - {end.ToString(dateFormat.Short)} ▾";

                foreach (var (start, end) in ranges)
                {
                    foreach (var dateFormat in dateFormats)
                    {
                        yield return new object[]
                        {
                            start,
                            end,
                            dateFormat,
                            expectedTitleString(start, end, dateFormat)
                        };
                    }
                }
            }
        }
    }
}

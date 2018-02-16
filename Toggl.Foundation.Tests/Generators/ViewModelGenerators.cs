using System;
using System.Linq;
using System.Reactive.Linq;
using FsCheck;
using MvvmCross.Core.Navigation;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Settings;
using MonthPredicate = System.Func<int, bool>;
using TimeEntry = Toggl.Foundation.Models.TimeEntry;

namespace Toggl.Foundation.Tests.Generators
{
    public static class ViewModelGenerators
    {
        public static Gen<TimeEntriesLogViewModel> ForTimeEntriesLogViewModel(MonthPredicate filter)
        {
            var now = new DateTimeOffset(2017, 08, 13, 08, 01, 23, TimeSpan.Zero);
            var monthsGenerator = Gen.Choose(1, 12).Where(filter);
            var yearGenerator = Gen.Choose(2007, now.Year);

            return Arb.Default
                .Array<DateTimeOffset>()
                .Generator
                .Select(dateTimes =>
                {
                    var source = Substitute.For<ITogglDataSource>();
                    var timeService = Substitute.For<ITimeService>();
                    var analyticsService = Substitute.For<IAnalyticsService>();
                    var onboardingStorage = Substitute.For<IOnboardingStorage>();
                    var navigationService = Substitute.For<IMvxNavigationService>();
                    var viewModel = new TimeEntriesLogViewModel(source, timeService, analyticsService, onboardingStorage, navigationService);

                    var year = yearGenerator.Sample(0, 1).First();

                    var observable = dateTimes
                        .Select(newDateWithGenerator(monthsGenerator, year))
                        .Select(d => TimeEntry.Builder
                                .Create(-1)
                                .SetUserId(-2)
                                .SetWorkspaceId(-3)
                                .SetStart(d)
                                .SetDescription("")
                                .SetAt(now).Build())
                        .Apply(Observable.Return);

                    source.TimeEntries.GetAll().Returns(observable);

                    return viewModel;
                });
        }

        private static Func<DateTimeOffset, DateTimeOffset> newDateWithGenerator(Gen<int> monthGenerator, int year)
        {
            var month = monthGenerator.Sample(0, 1).First();
            var day = Gen.Choose(1, DateTime.DaysInMonth(year, month)).Sample(0, 1).First();

            return dateTime =>
                new DateTime(year, month, day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
    }
}

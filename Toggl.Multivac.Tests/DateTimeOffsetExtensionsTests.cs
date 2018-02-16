using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Toggl.Multivac.Extensions;
using Xunit;

namespace Toggl.Multivac.Tests
{
    public sealed class DateTimeOffsetExtensionsTests
    {
        public sealed class TheRoundFunction
        {
            [Theory, LogIfTooSlow]
            [MemberData(nameof(SecondsWhichShouldBeRoundedDown))]
            public void ShouldRoundDownToTheNearestMinute(int second)
            {
                var time = new DateTimeOffset(2018, 02, 02, 6, 12, second, TimeSpan.Zero);

                var rounded = time.RoundToClosestMinute();

                rounded.Minute.Should().Be(time.Minute);
                rounded.Second.Should().Be(0);
            }

            [Theory, LogIfTooSlow]
            [MemberData(nameof(SecondsWhichShouldBeRoundedUp))]
            public void ShouldRoundUpToTheNearestMinute(int second)
            {
                var time = new DateTimeOffset(2018, 02, 02, 6, 12, second, TimeSpan.Zero);

                var rounded = time.RoundToClosestMinute();

                rounded.Minute.Should().Be(13);
                rounded.Second.Should().Be(0);
            }

            [Theory, LogIfTooSlow]
            [MemberData(nameof(SecondsWhichShouldBeRoundedUp))]
            public void ShouldRoundUpToTheNearestMinuteAndUpdateHourAndDayAndYearWhenNeeded(int second)
            {
                var time = new DateTimeOffset(2018, 12, 31, 23, 59, second, TimeSpan.Zero);

                var rounded = time.RoundToClosestMinute();

                rounded.Should().Be(new DateTimeOffset(2019, 01, 01, 00, 00, 00, TimeSpan.Zero));
            }

            public static IEnumerable<object[]> SecondsWhichShouldBeRoundedDown()
            {
                for (int i = 0; i < 30; i++)
                    yield return new object[] { i };
            }

            public static IEnumerable<object[]> SecondsWhichShouldBeRoundedUp()
            {
                for (int i = 30; i < 60; i++)
                    yield return new object[] { i };
            }
        }
    }
}

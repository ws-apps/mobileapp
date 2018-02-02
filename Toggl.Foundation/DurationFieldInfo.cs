using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation
{
    public struct DurationFieldInfo
    {
        public static DurationFieldInfo Empty = new DurationFieldInfo(new Stack<int>(maximumNumberOfDigits));

        private const int maximumNumberOfDigits = 5;

        private static readonly TimeSpan maximumDuration = TimeSpan.FromHours(MaxTimeEntryDurationInHours);

        private readonly Stack<int> digits;

        public int Minutes => combineDigitsIntoANumber(0, 2);

        public int Hours => combineDigitsIntoANumber(2, 3);

        private DurationFieldInfo(Stack<int> digits)
        {
            this.digits = digits;
        }

        public DurationFieldInfo Push(int digit)
        {
            if (digit < 0 || digit > 9)
                throw new ArgumentException($"Digits must be between 0 and 9, value {digit} was rejected.");

            if (digits.Count == maximumNumberOfDigits) return this;

            if (digits.Count == 0 && digit == 0) return this;

            var extendedStack = new Stack<int>(digits.Reverse());
            extendedStack.Push(digit);
            return new DurationFieldInfo(extendedStack);
        }

        public DurationFieldInfo Pop()
        {
            if (digits.Count == 0) return this;

            var reducedStack = new Stack<int>(digits.Reverse());
            reducedStack.Pop();
            return new DurationFieldInfo(reducedStack);
        }

        public static DurationFieldInfo FromTimeSpan(TimeSpan duration)
        {
            var totalMinutes = (long)duration.Clamp(TimeSpan.Zero, maximumDuration).TotalMinutes;
            var hoursPart = totalMinutes / 60;
            var minutesPart = totalMinutes % 60;
            var digitsString = (hoursPart * 100 + minutesPart).ToString();
            var digitsArray = digitsString.ToCharArray().Select(digit => digit - '0').ToArray();
            return new DurationFieldInfo(new Stack<int>(digitsArray));
        }

        public override string ToString()
            => $"{Hours:00}:{Minutes:00}";

        public TimeSpan ToTimeSpan()
            => TimeSpan.FromHours(Hours).Add(TimeSpan.FromMinutes(Minutes)).Clamp(TimeSpan.Zero, maximumDuration);

        private int combineDigitsIntoANumber(int start, int count)
        {
            var digitsArray = digits.ToArray();
            var number = 0;
            var power = 1;
            for (int i = start; i < Math.Min(start + count, digits.Count); i++)
            {
                number += digitsArray[i] * power;
                power *= 10;
            }

            return number;
        }
    }
}

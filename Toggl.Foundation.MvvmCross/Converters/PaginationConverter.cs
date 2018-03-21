﻿using System;
using System.Globalization;
using MvvmCross.Platform.Converters;

namespace Toggl.Foundation.MvvmCross.Converters
{
    public sealed class PaginationConverter<T> : MvxValueConverter<int, T>
    {
        private T[] pages;

        public PaginationConverter(T[] pages)
        {
            this.pages = pages;
        }

        protected override T Convert(int value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value < 0 || value >= pages.Length)
                throw new ArgumentOutOfRangeException($"{nameof(value)} must be within valud range of pages [0 - {pages.Length - 1}");

            return pages[value];
        }
    }
}

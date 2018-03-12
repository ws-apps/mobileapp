using System;
using MvvmCross.Core.ViewModels;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve]
    public sealed class SelectableBeginningOfWeekViewModel : MvxNotifyPropertyChanged
    {
        public BeginningOfWeek BeginningOfWeek { get; }

        public bool Selected { get; set; }

        public SelectableBeginningOfWeekViewModel(BeginningOfWeek beginningOfWeek, bool selected)
        {
            BeginningOfWeek = beginningOfWeek;
            Selected = selected;
        }
    }
}

using System;
namespace Toggl.Foundation.MvvmCross.Autocomplete
{
    public interface IAutocompleteEventProvider
    {
        event EventHandler TextChanged;
        event EventHandler ProjectDeleted;
        event EventHandler CursorPositionChanged;
        event EventHandler<TagDeletedEventArgs> TagDeleted;
    }
}

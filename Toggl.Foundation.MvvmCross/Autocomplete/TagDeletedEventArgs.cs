using System;
namespace Toggl.Foundation.MvvmCross.Autocomplete
{
    public class TagDeletedEventArgs : EventArgs
    {
        public int CursorPosition { get; }

        public int TagIndex { get; }

        public TagDeletedEventArgs(int cursorPosition, int tagIndex)
        {
            TagIndex = tagIndex;
            CursorPosition = cursorPosition;
        }
    }
}

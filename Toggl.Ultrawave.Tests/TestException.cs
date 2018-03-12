using System;

namespace Toggl.Ultrawave.Tests
{
    sealed class TestException : Exception
    {
        public TestException(string message)
            : base(message)
        {
        }
    }
}

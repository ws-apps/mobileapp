using System;

namespace Toggl.PrimeRadiant.Exceptions
{
    public sealed class EntityNotFoundException : DatabaseException
    {
        private const string defaultErrorMessage = "Entity not found in database";

        public EntityNotFoundException(Exception exception)
            : base($"{defaultErrorMessage}: {exception.Message}", exception)
        {
        }
    }
}

using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public sealed class UserIsMissingApiTokenException : ServerErrorException
    {
        private const string defaultMessage = "The API returned a user with no api token.";
        
        internal UserIsMissingApiTokenException(IRequest request, IResponse response)
            : base(request, response, defaultMessage)
        {
        }
    }
}

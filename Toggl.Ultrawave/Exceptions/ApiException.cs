using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Exceptions
{
    public class ApiException : Exception
    {
        internal IRequest Request { get; }

        internal IResponse Response { get; }

        private readonly string message;

        internal ApiException(IRequest request, IResponse response, string message)
        {
            Request = request;
            Response = response;
            this.message = message;
        }

        public string LocalizedApiErrorMessage
            => Response.RawData;

        public override string ToString()
            => $"{GetType().Name} for request {Request.HttpMethod} {Request.Endpoint}: "
                + $"Response: "
                + $"(Status: [{(int)Response.StatusCode} {Response.StatusCode}]) "
                + $"(Headers: [{SerializeHeaders(Response.Headers)}]) "
                + $"(Body: {Response.RawData}) "
                + $"(Message: {message})";

        internal static string SerializeHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
            => String.Join(", ", headers.Select(pair => $"'{pair.Key}': [{String.Join(", ", pair.Value.Select(v => $"'{v}'").ToArray())}]").ToArray());

        public override string Message => ToString();
    }
}

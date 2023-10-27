using System.Net.Http;

namespace Maverick.Client.ArangoDb.Protocol;

internal class RequestHelper
{
    internal static HttpRequestMessage Clone(HttpRequestMessage originalRequest)
    {
        var msg = new HttpRequestMessage
        {
            Content = originalRequest.Content,
            Method = originalRequest.Method,
            RequestUri = originalRequest.RequestUri,
            Version = originalRequest.Version
        };

        return msg;
    }
}

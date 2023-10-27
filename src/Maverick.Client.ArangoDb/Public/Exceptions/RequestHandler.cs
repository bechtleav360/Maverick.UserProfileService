using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Exceptions;

/// <summary>
///     Helper class used to send request with execption handling.
/// </summary>
internal class RequestHandler
{
    private static async Task<Response> ExecuteInternalAsync(
        Connection connection,
        Request request,
        bool forceDirtyRead = false,
        Response response = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        response ??= new Response();

        try
        {
            response = await connection.SendAsync(request, forceDirtyRead, timeout, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ConnectionFailedException connExc)
        {
            connExc.OperationUri ??= request.OperationUri;
            connExc.UsedHttpMethod ??= request.HttpMethod.ToString("G");
            response.Exception = connExc;
        }
        catch (Exception ex)
        {
            response.Exception ??= new Exception(
                "An error occurred while sending request. Could not be mapped as arango error.",
                ex);
        }

        return response;
    }

    internal static Task<Response> ExecuteAsync<T>(
        Connection connection,
        Request<T> request,
        bool forceDirtyRead = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            connection,
            request,
            Serializer.GetDefaultInternalJsonSettings(),
            forceDirtyRead,
            timeout,
            cancellationToken);
    }

    internal static async Task<Response> ExecuteAsync<T>(
        Connection connection,
        Request<T> request,
        JsonSerializerSettings serializerSettings,
        bool forceDirtyRead = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "The request object is null, but must be set!");
        }

        var response = new Response
        {
            DebugInfo = new DebugInfo
            {
                RequestUri = request.OperationUri,
                RequestHttpMethod = request.HttpMethod.ToString()
            },
            IsSuccessStatusCode = false
        };

        try
        {
            request.SerializeBody(serializerSettings);
            response.DebugInfo.RequestJsonBody = request.BodyAsString;
        }
        catch (Exception ex)
        {
            response.Exception = new Exception($"Error by serializing of: {request.GetType()}", ex);

            return response;
        }

        response = await ExecuteInternalAsync(
            connection,
            request,
            forceDirtyRead,
            response,
            timeout,
            cancellationToken);

        return response;
    }

    internal static async Task<Response> ExecuteAsync(
        Connection connection,
        Request request,
        bool forceDirtyRead = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "The request object is null, but must be set!");
        }

        Response response = await ExecuteInternalAsync(
            connection,
            request,
            forceDirtyRead,
            timeout: timeout,
            cancellationToken: cancellationToken);

        return response;
    }
}

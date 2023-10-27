using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol.Extensions;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Protocol;

internal class EndpointsManager
{
    /// <summary>
    ///     The default host that is used to send request to the arango API.
    /// </summary>
    private string _defaultHost;

    /// <summary>
    ///     Queue of endpoints
    /// </summary>
    private readonly Queue<string> _endpointsAddresses;

    public EndpointsManager(Queue<string> endpointsAddresses, bool withActiveFailover = false)
    {
        _endpointsAddresses = endpointsAddresses
            ?? throw new Exception("Arango endpoint manager: No endpoint addresses provided!");
    }

    private static void InitializeClient(HttpClient httpClient, string username, string password)
    {
        httpClient.DefaultRequestHeaders.Authorization ??= new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
    }

    private static HttpRequestMessage Clone(HttpRequestMessage originalRequest)
    {
        var msg = new HttpRequestMessage
        {
            Content = originalRequest.Content,
            Method = originalRequest.Method,
            RequestUri = originalRequest.RequestUri,
            Version = originalRequest.Version
        };

        if (!originalRequest.Options.Any())
        {
            return msg;
        }

        foreach ((string key, object value) in originalRequest.Options)
        {
            // just to be on the safe side
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            // maybe some properties have been set by the constructor (to be on the safe side)
            msg.Options.Add(key, value);
        }

        return msg;
    }

    /// <summary>
    ///     Send the request to the server API
    /// </summary>
    /// <param name="request">The request to be sent.</param>
    /// <param name="httpClient">The client to be used to send the request.</param>
    /// <param name="username">The user name to be authenticates.</param>
    /// <param name="password">The password to be authenticate.</param>
    /// <param name="databaseName">The name of the database the request is related to.</param>
    /// <param name="logger">The <see cref="ILogger" /> th will take log message of this methods (optional).</param>
    /// <param name="forceDirtyRead"> send request to a follower (only available in the active-failover setup)</param>
    /// <param name="timeout">
    ///     sets the timespan to wait before the request times out (default: null, that means default value
    ///     of HTTP clients will be taken.)
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> TrySendAsync(
        Request request,
        HttpClient httpClient,
        string username,
        string password,
        string databaseName,
        ILogger logger = null,
        bool forceDirtyRead = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(
                nameof(request),
                "Arango endpoint manager: A Request with the value null can not be operated");
        }

        if (httpClient == null)
        {
            throw new ArgumentNullException(
                nameof(httpClient),
                "Arango endpoint manager: The httpClient should not be null");
        }

        if (username == null)
        {
            throw new ArgumentNullException(
                nameof(username),
                "Arango endpoint manager: Username should not be null or empty");
        }

        if (password == null)
        {
            throw new ArgumentNullException(
                nameof(password),
                "Arango endpoint manager: Password should not be null or empty");
        }

        if (string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentNullException(
                nameof(databaseName),
                "Arango endpoint manager: The database name should be null or empty");
        }

        var requestMessage = new HttpRequestMessage
        {
            Content = request.BodyAsString != null ? new StringContent(request.BodyAsString) : null,
            Method = new System.Net.Http.HttpMethod(request.HttpMethod.ToString())
        };

        // The timeout property is part of a custom logic.
        // Only the appropriate HTTP message handler will use it
        // and should be passed to the HTTP client (either via HttClientFactory setup or new HttpClient(handler).
        // The default handlers will ignore it.
        requestMessage.SetTimeout(timeout);
        requestMessage.StoreTransactionInformation(request.TransactionInformation);
        WebHeaderCollection headerCollection = request.Headers;

        for (var index = 0; index < headerCollection.Count; index++)
        {
            
            string headerName = headerCollection.GetKey(index);
            string headerValue = headerCollection.Get(index);

            if (httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                    headerName,
                    headerValue))
            {
                logger?.LogInformation(
                    "Header with name:{headerName}, has been added with following value: {headerValue}",
                    headerName,headerValue);
            }
            else
            {
                logger?.LogWarning(
                    "Header with name:{headerName}, and value: {headerValue} could not be added",
                    headerName, headerValue);
            }

        }

        for (var i = 0; i < _endpointsAddresses.Count; i++)
        {
            InitializeClient(httpClient, username, password);
            _defaultHost ??= _endpointsAddresses.Peek();

            try
            {
                requestMessage.RequestUri =
                    new Uri(_defaultHost + "_db/" + databaseName + "/" + request.GetRelativeUri());

                HttpResponseMessage response = await httpClient.SendAsync(
                    Clone(requestMessage),
                    cancellationToken);

                httpClient.DefaultRequestHeaders.Clear();

                if (!response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable))
                {
                    return response;
                }

                // extract the correct endpoint
                logger?.LogInformation(
                    "Request has been sent to follower :{host}, retrieving leader address from follower header.",
                    _defaultHost);

                string responseAsString = await response?.Content?.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<ArangoErrorResponse>(responseAsString);

                if (!(result is { ErrorNum: AStatusCode.ErrorClusterNotLeader }))
                {
                    return response;
                }

                if (response.Headers.TryGetValues(
                        ParameterName.HeaderEndpoint,
                        out IEnumerable<string> leaderAddress))
                {
                    _defaultHost = leaderAddress.FirstOrDefault() + "/";

                    logger?.LogWarning(
                        "Request has been sent to follower, current leader address is: {host}",
                        _defaultHost);

                    requestMessage.RequestUri =
                        new Uri(_defaultHost + "_db/" + databaseName + "/" + request.GetRelativeUri());

                    InitializeClient(httpClient, username, password);

                    return await httpClient.SendAsync(Clone(requestMessage), cancellationToken);
                }

                logger?.LogWarning(
                    "Arango endpoint manager: Request has been sent to a follower - Extraction of leader address from response header was not successful");

                return response;
            }
            catch (HttpRequestException)
            {
                logger?.LogWarning("The host: {host} is not responding", _defaultHost);
                _endpointsAddresses.Enqueue(_endpointsAddresses.Dequeue());
                _defaultHost = null;
            }
            catch (JsonException ex)
            {
                logger?.LogError(ex, "Error happened during deserializing Arango error response");
            }
            catch (Exception exc)
            {
                logger?.LogError(
                    exc,
                    "Error: {excMessage} happened during sending request with the host: {host}",
                    exc.Message,
                    _defaultHost);
            }
            finally
            {
                httpClient.DefaultRequestHeaders.Clear();
            }
        }

        // if no configured host endpoint is working/reachable, throw an exception
        throw new ConnectionFailedException(
            $"No reachable host in the connection string. Tried endpoints: {string.Join("; ", _endpointsAddresses)}",
            _endpointsAddresses);
    }
}

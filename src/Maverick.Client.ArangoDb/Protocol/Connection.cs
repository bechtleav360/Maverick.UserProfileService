using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol.Extensions;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Exceptions;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Handlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Boolean;
using static Maverick.Client.ArangoDb.Public.AConstants;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Protocol;

/// <summary>
///     Stores data about single endpoint and processes communication between client and server.
/// </summary>
internal class Connection : AbstractConnection, IDisposable
{
    // Only used when calling Create() of HttpClientFactory
    private string _clientName;

    private EndpointsManager _endpointsManager;
    private HttpClient _httpClient;
    private readonly ILogger _logger;

    public JsonSerializerSettings DefaultSerializerSettings { get; private set; }
    public Queue<string> EndpointsAddresses { get; set; } = new Queue<string>();

    public ArangoExceptionOptions ExceptionOptions { get; } = new ArangoExceptionOptions();

    /// <summary>
    ///     initialize the connection with the following parameters
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    /// <param name="isSecured"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="exceptionOptions"></param>
    /// <param name="useWebProxy"></param>
    /// <param name="debug"></param>
    [Obsolete(
        "use the method Connection(string connectionString, IHttpClientFactory clientFactory = null, ILogger logger = null)")]
    internal Connection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        string userName,
        string password,
        ArangoExceptionOptions exceptionOptions = null,
        bool useWebProxy = false,
        bool debug = false)
    {
        Alias = alias;
        Hostname = hostname;
        Port = port;
        IsSecured = isSecured;
        Username = userName;
        Password = password;
        Debug = debug;
        ExceptionOptions = exceptionOptions;

        UseWebProxy = useWebProxy;

        BaseUri = new Uri((isSecured ? "https" : "http") + "://" + hostname + ":" + port + "/");
    }

    [Obsolete(
        "use the method Connection(string connectionString, IHttpClientFactory clientFactory = null, ILogger logger = null)")]
    internal Connection(
        string alias,
        string hostname,
        int port,
        bool isSecured,
        string databaseName,
        string userName,
        string password,
        ArangoExceptionOptions exceptionOptions = null,
        bool useWebProxy = false,
        bool debug = false)
        : this(
            alias,
            hostname,
            port,
            isSecured,
            userName,
            password,
            exceptionOptions,
            useWebProxy,
            debug)
    {
        DatabaseName = databaseName;

        BaseUri = new Uri(
            (isSecured ? "https" : "http") + "://" + hostname + ":" + port + "/_db/" + databaseName + "/");
    }

    internal Connection(
        string alias,
        string connectionString,
        string databaseName,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        IHttpClientFactory clientFactory = null)
        : this(alias, connectionString, exceptionOptions, defaultSerializerSettings, clientFactory)
    {
        DatabaseName = databaseName;

        if (clientFactory == null)
        {
            _httpClient = new HttpClient();
        }
        else
        {
            ClientFactory = clientFactory;
        }

        BaseUri = new Uri(
            (IsSecured ? "https" : "http") + "://" + Hostname + ":" + Port + "/_db/" + databaseName + "/");
    }

    internal Connection(
        string connectionString,
        HttpClient client,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        ILogger logger = null)
        : this(
            connectionString,
            null,
            client,
            logger,
            exceptionOptions,
            defaultSerializerSettings)
    {
    }

    internal Connection(
        string connectionString,
        IHttpClientFactory httpClientFactory,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        ILogger logger = null,
        string clientName = ArangoClientName)
        : this(
            connectionString,
            httpClientFactory,
            null,
            logger,
            exceptionOptions,
            defaultSerializerSettings)
    {
        _clientName = clientName ?? ArangoClientName; // to be sure, it is set
    }

    internal Connection(
        string alias,
        string connectionString,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null,
        IHttpClientFactory clientFactory = null)
        : this(
            connectionString,
            clientFactory,
            null,
            exceptionOptions: exceptionOptions,
            defaultSerializerSettings: defaultSerializerSettings)
    {
        Alias = alias;
    }

    private Connection()
    {
    }

    private Connection(
        string connectionString,
        IHttpClientFactory clientFactory = null,
        HttpClient client = null,
        ILogger logger = null,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings defaultSerializerSettings = null)
    {
        DefaultSerializerSettings = defaultSerializerSettings;
        ExceptionOptions = exceptionOptions ?? new ArangoExceptionOptions();

        string endpointsListAsString = ParseString(connectionString, "Endpoints");

        if (string.IsNullOrWhiteSpace(endpointsListAsString))
        {
            throw new ArgumentException("no endpoints in the connection string");
        }

        TryParse(ParseString(connectionString, "ActiveFailover", false), out bool failover);
        ActiveFailOver = failover;

        _endpointsManager = new EndpointsManager(GetQueueFromString(endpointsListAsString), ActiveFailOver);
        EndpointsAddresses = GetQueueFromString(endpointsListAsString);

        if (EndpointsAddresses == null || EndpointsAddresses.Count < 1)
        {
            throw new FormatException("Endpoints are missing in the connection string");
        }

        if (EndpointsAddresses.Count < 2 && ActiveFailOver)
        {
            throw new ArgumentException("Active Failover mode need at least 2 hosts");
        }

        // set Debug and UseWebProxy to an optional parameter
        TryParse(ParseString(connectionString, "Debug", false), out bool debug);
        Debug = debug;

        TryParse(ParseString(connectionString, "UseWebProxy", false), out bool proxy);
        UseWebProxy = proxy;

        DatabaseName = ParseString(connectionString, "Database");

        if (string.IsNullOrEmpty(DatabaseName))
        {
            throw new FormatException("Database name is missing in the connection string");
        }

        Username = ParseString(connectionString, "UserName");

        if (string.IsNullOrEmpty(Username))
        {
            throw new FormatException("Username is missing in the connection string.");
        }

        Password = ParseString(connectionString, "Password");
        _logger = logger;

        if (clientFactory != null)
        {
            ClientFactory = clientFactory;

            return;
        }

        if (client != null)
        {
            _httpClient = client;

            return;
        }

        // Note that we need to disable the HttpClient’s timeout by setting it to an infinite value,
        // otherwise the default behavior will interfere with the timeout handler.
        _httpClient = new HttpClient(new TimeoutHttpHandler())
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    private async Task HandleException(Exception exception)
    {
        if (exception != null && ExceptionOptions.ExceptionHandler != null)
        {
            _logger?.LogInformation("Invoke custom exception handler of exception options.");
            await ExceptionOptions.ExceptionHandler.Invoke(exception);
        }
    }

    private async Task<Response> ExecuteWithRetryAsync(Func<Task<Response>> action)
    {
        bool retry = ExceptionOptions.RetryEnabled;
        var currentRetry = 0;

        while (retry && currentRetry <= ExceptionOptions.RetryCount)
        {
            Response result = await action.Invoke();

            // If no retry exception occurred, or it is last retry.
            if (result.Exception?.IsARetryError() != true || currentRetry == ExceptionOptions.RetryCount)
            {
                return result;
            }

            await Task.Delay(ExceptionOptions.SleepDuration.Invoke(currentRetry));
            currentRetry++;
        }

        // Disabled 
        return await action.Invoke();
    }

    private static ApiErrorException TryParseException(HttpResponseMessage response, string body)
    {
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ArangoErrorResponse>(body);

                return new ApiErrorException(
                    $"An error occurred while sending the request. Arango has returned a corresponding error code at {errorResponse.ErrorNum}.",
                    errorResponse);
            }
            catch (Exception e)
            {
                return new ApiErrorException(
                    "An error occurred while sending the request. Error response could not be deserialized.",
                    e);
            }
        }

        // If no error has occurred, null is returned.
        return null;
    }

    internal Connection Clone()
    {
        return new Connection
        {
            Alias = Alias,
            BaseUri = BaseUri,
            DatabaseName = DatabaseName,
            Debug = Debug,
            Hostname = Hostname,
            IsSecured = IsSecured,
            Password = Password,
            Port = Port,
            UseWebProxy = UseWebProxy,
            Username = Username,
            ClientFactory = ClientFactory,
            DefaultSerializerSettings = DefaultSerializerSettings,
            _endpointsManager = _endpointsManager,
            _httpClient = _httpClient,
            _clientName = _clientName
        };
    }

    internal async Task<Response> SendAsync(
        Request request,
        bool forceDirtyRead = false,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpClient httpClient = ClientFactory != null
                ? ClientFactory.CreateClient(_clientName)
                : _httpClient;

            Response response;

            if (Debug)
            {
                response = await ExecuteWithRetryAsync(
                    async () =>
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        HttpResponseMessage innerResponse = await _endpointsManager.TrySendAsync(
                            request,
                            httpClient,
                            Username,
                            Password,
                            DatabaseName,
                            _logger,
                            forceDirtyRead,
                            timeout,
                            cancellationToken);

                        stopwatch.Stop();

                        return await ParseResponseAsync(
                            innerResponse,
                            DefaultSerializerSettings,
                            stopwatch.ElapsedMilliseconds);
                    });
            }
            else
            {
                response = await ExecuteWithRetryAsync(
                    async () =>
                    {
                        HttpResponseMessage innerResponse = await _endpointsManager.TrySendAsync(
                            request,
                            httpClient,
                            Username,
                            Password,
                            DatabaseName,
                            _logger,
                            forceDirtyRead,
                            timeout,
                            cancellationToken);

                        return await ParseResponseAsync(
                            innerResponse,
                            DefaultSerializerSettings);
                    });
            }

            await HandleException(response.Exception);

            return response;
        }
        catch (Exception e)
        {
            _logger?.LogError(
                e,
                "An error has occurred that was not thrown by the arango. It indicates a connection error.");

            await HandleException(e);

            throw;
        }
    }

    internal static string ParseString(string input, string output, bool throwException = true)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = input
        };

        // In this method 'throwException' can be treated like 'isRequired',
        // because it will fail in the try...catch block anyway, if the provided key is not present
        if (!builder.ContainsKey(output) && throwException)
        {
            throw new FormatException(
                $"Error occurred during parsing connection string, because mandatory property '{output}' is missing. Connection string is not valid.");
        }

        if (!builder.ContainsKey(output))
        {
            return default;
        }

        try
        {
            return builder[output].ToString();
        }
        catch (ArgumentException arEx)
        {
            if (throwException)
            {
                throw new FormatException(
                    $"Error occurred during parsing connection string '{input}' to get property '{output}'. Connection string is not valid. {arEx.Message}",
                    arEx);
            }

            return default;
        }
    }

    /// <summary>
    ///     This is an help method to transform a listing from character separated with a delimiter to a queue of string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static Queue<string> GetQueueFromString(string input)
    {
        IEnumerable<string> outputs = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(output => output.Trim().EndsWith("/") ? output : $"{output}/");

        return new Queue<string>(outputs);
    }

    internal static async Task<Response> ParseResponseAsync(
        HttpResponseMessage response,
        JsonSerializerSettings defaultSerializerSettings = null,
        long requestExecutionTime = 0)
    {
        if (response?.Content == null)
        {
            throw new ArgumentException("Response and its content must be not null!");
        }

        string body = await response.Content?.ReadAsStringAsync();

        var finalResponse = new Response
        {
            DefaultSerializerSettings = defaultSerializerSettings,
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            ResponseHeaders = response.Headers,
            ResponseBodyAsString = body,
            RequestMessage = response.RequestMessage,
            Exception = TryParseException(response, body),
            DebugInfo = new DebugInfo
            {
                RequestUri = response.RequestMessage?.RequestUri?.OriginalString,
                RequestHttpMethod = response.RequestMessage?.Method?.Method,
                RequestJsonBody =
                    response?.RequestMessage?.Content != null
                        ? await response.RequestMessage.Content.ReadAsStringAsync()
                        : default,
                ExecutionTime = requestExecutionTime,
                TransactionId = response.RequestMessage.ExtractTransactionId()
            }
        };

        return finalResponse;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

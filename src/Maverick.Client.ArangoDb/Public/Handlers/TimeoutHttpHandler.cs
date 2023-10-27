using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.Logging;

namespace Maverick.Client.ArangoDb.Public.Handlers;

/// <summary>
///     An HTTP message handler that can be chained to other handlers.
///     Throws a timeout exception, when a default or specific timeout has been reached.
///     It logs http requests including their transaction id.
/// </summary>
public class TimeoutHttpHandler : DelegatingHandler
{
    private readonly ILogger<TimeoutHttpHandler> _logger;

    /// <summary>
    ///     Creates a <see cref="TimeoutHttpHandler" /> with embedded <see cref="HttpClientHandler" />.
    /// </summary>
    public TimeoutHttpHandler Default =>
        new TimeoutHttpHandler
        {
            InnerHandler = new HttpClientHandler()
        };

    /// <summary>
    ///     Default timeout property to our handler; it will be used for requests that don’t have their timeout explicitly set.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    ///     Initializes a new instance of <exception cref="TimeoutHttpHandler"></exception>
    /// </summary>
    /// <param name="logger">An optional logger instance that will take logging messages.</param>
    public TimeoutHttpHandler(ILogger<TimeoutHttpHandler> logger = null)
    {
        _logger = logger;
    }

    private CancellationTokenSource GetCancellationTokenSource(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        TimeSpan timeout = request.GetTimeout() ?? DefaultTimeout;

        if (timeout == Timeout.InfiniteTimeSpan)
        {
            // No need to create a CTS if there's no timeout
            return null;
        }

        var cts = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);

        cts.CancelAfter(timeout);

        return cts;
    }

    /// <inheritdoc />
    /// <exception cref="TimeoutException">Executing the requests has been taken too long. Specified timeout has been reached.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="request" /> is <c>null</c>.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // only set a request id, if the logger is available and will take the debug log message
        string requestId =
            _logger != null && _logger.IsEnabled(LogLevel.Trace)
                ? Guid.NewGuid().ToString("N")
                : string.Empty;

        using CancellationTokenSource cts = GetCancellationTokenSource(request, cancellationToken);

        try
        {
            _logger?.LogTrace(
                "Sending HTTP request with id: {requestId}: {} {} (transaction id: {transactionId}, info: {})",
                requestId,
                request.Method,
                request.RequestUri,
                request.ExtractTransactionId(),
                request.GetTransactionInformation() ?? "<NOT_SET>");

            HttpResponseMessage response = await base.SendAsync(
                request,
                cts?.Token ?? cancellationToken);

            _logger?.LogTrace(
                "Got a response for request with id {requestId}. HTTP response code: {}",
                requestId,
                response?.StatusCode.ToString("D") ?? "UNKNOWN");

            return response;
        }
        // this way we don’t actually catch the OperationException when we want to let it propagate,
        // and we avoid unnecessarily unwinding the stack.
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            string message = string.Concat(
                "Timeout error during execution of the request",
                string.IsNullOrEmpty(requestId) ? " " : $" with id {requestId} ",
                $"({request.Method} {request.RequestUri}).");

            throw new TimeoutException(message);
        }
    }
}

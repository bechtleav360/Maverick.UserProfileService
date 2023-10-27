using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UserProfileService.Common.V2.RequestBuilder;
using UserProfileService.Configuration;

namespace UserProfileService.Services;

/// <summary>
///     Defines a base class for services which are sending internal requests.
/// </summary>
public abstract class BaseSyncRequesterService
{
    protected readonly IHttpClientFactory ClientFactory;

    /// <summary>
    ///     The requests builder, that generates the requests.
    /// </summary>
    protected BasicRequestBuilder RequestBuilder { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="BaseSyncRequesterService" />
    /// </summary>
    /// <param name="synConfig"></param>
    /// <param name="clientFactory"></param>
    /// <param name="loggerFactory"></param>
    protected BaseSyncRequesterService(
        IOptions<SyncOptions> synConfig,
        IHttpClientFactory clientFactory,
        ILoggerFactory loggerFactory)
    {
        ClientFactory = clientFactory;

        RequestBuilder = new BasicRequestBuilder(
            new Uri(synConfig.Value.Endpoint, UriKind.Absolute),
            loggerFactory);
    }

    /// <summary>
    ///     Deserialize the response to the right object format.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="responseMessage">The original response message.</param>
    /// <param name="logger">An instance of <see cref="ILogger" /></param>
    /// <param name="serializerSettings">The json setting that are used to deserialize the response messages.</param>
    /// <param name="caller">The caller method, that calls the actual method.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>Returns the response in the right object format.</returns>
    protected async Task<T> DeserializeSafely<T>(
        HttpResponseMessage responseMessage,
        ILogger logger,
        JsonSerializerSettings serializerSettings = null,
        [CallerMemberName] string caller = null,
        CancellationToken cancellationToken = default)
    {
        if (responseMessage?.Content == null)
        {
            logger.LogDebug(
                "{caller}(): Response message is empty or null. Default instance of {type} will be returned.",
                caller,
                typeof(T).Name);

            return default;
        }

        try
        {
            string json = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            logger.LogTrace(
                "{caller}(): Cancellation token is cancellation requested: {isCancellationRequested}.",
                caller,
                cancellationToken.IsCancellationRequested);

            cancellationToken.ThrowIfCancellationRequested();

            return JsonConvert.DeserializeObject<T>(
                json,
                serializerSettings);
        }
        catch (OperationCanceledException e)
        {
            logger.LogError(
                e,
                "{caller}(): Response message is empty or null. Default instance of {type} will be returned.",
                caller,
                typeof(T).Name);

            return default;
        }
        catch (Exception e)
        {
            logger.LogError(e, "{caller}(): An error occurred while deserializing object.", caller);

            return default;
        }
    }

    /// <summary>
    ///     Check the validity of the request message
    /// </summary>
    /// <param name="requestMessage">An instance of <see cref="HttpRequestMessage" /></param>
    /// <exception cref="ArgumentNullException"> will be thrown when the request message or the uri are not set (null)</exception>
    /// <exception cref="ArgumentException"> will be thrown when the uri contained in the request message is not valid</exception>
    protected internal virtual void CheckRequestMessage(HttpRequestMessage requestMessage)
    {
        if (requestMessage == null)
        {
            throw new ArgumentNullException(nameof(requestMessage));
        }

        if (requestMessage.RequestUri == null)
        {
            throw new ArgumentNullException(nameof(requestMessage.RequestUri));
        }

        if (!Uri.IsWellFormedUriString(requestMessage.RequestUri.OriginalString, UriKind.RelativeOrAbsolute))
        {
            throw new ArgumentException($"{nameof(requestMessage.RequestUri)} is not a valid Uri");
        }
    }
}

using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Configuration;
using UserProfileService.Proxy.Sync.Abstractions;
using UserProfileService.Proxy.Sync.Models;
using UserProfileService.Proxy.Sync.Utilities;
using UserProfileService.Utilities;

namespace UserProfileService.Services;

/// <summary>
///     An implementation of <see cref="ISynchronizationService" /> to handle operations on the UPS-Sync through the
///     UPS-API.
/// </summary>
public class ApiSynchronizationService : BaseSyncRequesterService, ISynchronizationService
{
    private readonly ILogger<ApiSynchronizationService> _logger;

    /// <summary>
    ///     Creates an instance of <see cref="ApiSynchronizationService" />
    /// </summary>
    /// <param name="clientFactory">The client factory used to generate http client which send requests.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="synConfig">The sync config.</param>
    public ApiSynchronizationService(
        IOptions<SyncOptions> synConfig,
        IHttpClientFactory clientFactory,
        ILoggerFactory loggerFactory) : base(synConfig, clientFactory, loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ApiSynchronizationService>();
    }

    /// <inheritdoc />
    public async Task<Guid> StartSynchronizationAsync(
        string correlationId,
        bool schedule,
        CancellationToken cancellationToken)
    {
        _logger.EnterMethod();

        HttpRequestMessage requestMessage =
            RequestBuilder.SetMethod(HttpMethod.Post)
                .SetUri("/Sync")
                .AddQueryParameter(nameof(schedule), schedule.ToString().ToLower())
                .AddHeaders(Constants.HeaderNameCorrelationId, correlationId, false)
                .BuildRequest();

        CheckRequestMessage(requestMessage);

        _logger.LogInfoMessage(
            "Getting sync process from UPS-Sync with following parameter: correlation Id: {correlationId}, schedule: {schedule}",
            LogHelpers.Arguments(correlationId, schedule));

        HttpResponseMessage response = await ClientFactory.CreateClient(SyncConstants.SyncClient)
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInfoMessage(
            "Get internal responses from UPS-Sync with status code {statusCode}",
            LogHelpers.Arguments(response.StatusCode));

        (bool wasSuccessFul, string errorMessage) = await ParseErrorMessageFromResponseAsync(
            response,
            cancellationToken);

        if (!wasSuccessFul)
        {
            _logger.LogWarnMessage(
                "Did not get a successful response from UPS-Sync: Got status code {responseHttpStatusCode} {errorMessage}",
                LogHelpers.Arguments(response.StatusCode, errorMessage)); 
            return _logger.ExitMethod(Guid.Empty);
        }

        string syncProcessIdString = response.Headers.Location?.Segments.LastOrDefault();

        if (string.IsNullOrWhiteSpace(syncProcessIdString))
        {
            _logger.LogWarnMessage(
                "Error happened by extraction of response from UPS-Sync: Sync process id was not found in location header of response",
                LogHelpers.Arguments());
        }

        if (Guid.TryParse(syncProcessIdString, out Guid syncProcessId))
        {
            return _logger.ExitMethod(syncProcessId);
        }

        _logger.LogWarnMessage(
            "Error happened by extraction of response from UPS-Sync: A value of sync process id was found in location header of response, but it was in a wrong format (value: {locationValue}).",
            LogHelpers.Arguments(syncProcessIdString));

        return _logger.ExitMethod(Guid.Empty);
    }

    /// <inheritdoc />
    public async Task<ProcessView> GetProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        HttpRequestMessage requestMessage =
            RequestBuilder.SetMethod(HttpMethod.Get).SetUri($"/Process/{id}").BuildRequest();

        CheckRequestMessage(requestMessage);

        _logger.LogInfoMessage(
            "Getting sync process with {id} from UPS-Sync with following request uri {uri}",
            LogHelpers.Arguments(id, requestMessage.RequestUri));

        HttpResponseMessage response = await ClientFactory.CreateClient(SyncConstants.SyncClient)
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInfoMessage(
            "Get internal responses from UPS-Sync with status code {statusCode}",
            LogHelpers.Arguments(response.StatusCode));

        return _logger.ExitMethod(
            await DeserializeSafely<ProcessView>(response, _logger, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ProcessDetail> GetDetailedProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        HttpRequestMessage requestMessage =
            RequestBuilder.SetMethod(HttpMethod.Get).SetUri($"/Process/{id}/detail").BuildRequest();

        _logger.LogInfoMessage(
            "Getting detailed sync process with {id} from UPS-Sync with following request uri {uri}",
            LogHelpers.Arguments(id, requestMessage.RequestUri));

        HttpResponseMessage response = await ClientFactory.CreateClient(SyncConstants.SyncClient)
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);

        CheckRequestMessage(requestMessage);

        _logger.LogInfoMessage(
            "Get internal responses from UPS-Sync with status code {statusCode}",
            LogHelpers.Arguments(response.StatusCode));

        return _logger.ExitMethod(
            await DeserializeSafely<ProcessDetail>(response, _logger, cancellationToken: cancellationToken));
    }
    
    /// <inheritdoc />
    public async Task<PaginatedListResponse<ProcessView>> GetAllProcessesAsync(
        QueryObject queryObject,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        HttpRequestMessage requestMessage =
            RequestBuilder.SetMethod(HttpMethod.Get)
                .SetUri("/Process")
                .AddQueryParameter(nameof(QueryObject.OrderedBy), queryObject.OrderedBy)
                .AddQueryParameter(nameof(QueryObject.SortOrder), queryObject.SortOrder.ToString())
                .AddQueryParameter(nameof(QueryObject.Page), $"{queryObject.Page}")
                .AddQueryParameter(nameof(QueryObject.PageSize), $"{queryObject.PageSize}")
                .BuildRequest();

        CheckRequestMessage(requestMessage);

        _logger.LogInfoMessage(
            "Getting all sync processes from UPS-Sync with following request uri {uri}",
            LogHelpers.Arguments(requestMessage.RequestUri));

        HttpResponseMessage response = await ClientFactory.CreateClient(SyncConstants.SyncClient)
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInfoMessage(
            "Get internal responses from UPS-Sync with status code {statusCode}",
            LogHelpers.Arguments(response.StatusCode));

        return _logger.ExitMethod(
            await DeserializeSafely<PaginatedListResponse<ProcessView>>(
                response,
                _logger,
                cancellationToken: cancellationToken));
    }

    private static async Task<(bool wasSuccessful, string errorMessage)> ParseErrorMessageFromResponseAsync(
        HttpResponseMessage message,
        CancellationToken cancellationToken)
    {
        if (message.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }

        try
        {
            string errorMessage = await message.Content.ReadAsStringAsync(cancellationToken);

            return (false, errorMessage);
        }
        catch
        {
            return (false, string.Empty);
        }
    }
}

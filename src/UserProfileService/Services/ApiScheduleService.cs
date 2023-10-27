using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Configuration;
using UserProfileService.Proxy.Sync.Abstractions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Requests;
using UserProfileService.Utilities;

namespace UserProfileService.Services;

/// <summary>
///     An implementation of <see cref="IScheduleService" />.
/// </summary>
public class ApiScheduleService : BaseSyncRequesterService, IScheduleService
{
    private readonly ILogger<ApiScheduleService> _logger;

    /// <summary>
    ///     Creates a new instance of <see cref="ApiScheduleService" />
    /// </summary>
    /// <param name="synConfig">The Sync endpoint configuration</param>
    /// <param name="clientFactory"> An http client factory</param>
    /// <param name="loggerFactory">A logger factory</param>
    public ApiScheduleService(
        IOptions<SyncOptions> synConfig,
        IHttpClientFactory clientFactory,
        ILoggerFactory loggerFactory) : base(synConfig, clientFactory, loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ApiScheduleService>();
    }

    /// <inheritdoc />
    public async Task<SyncSchedule> GetScheduleAsync(CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        HttpRequestMessage requestMessage =
            RequestBuilder.SetMethod(HttpMethod.Get)
                .SetUri("/Schedule")
                .BuildRequest();

        _logger.LogInfoMessage(
            "Getting schedule from ups sync with the request:{uri}",
            LogHelpers.Arguments(requestMessage.RequestUri?.ToString()));

        HttpResponseMessage response = await ClientFactory.CreateClient(SyncConstants.SyncClient)
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);

        CheckRequestMessage(requestMessage);

        _logger.LogInfoMessage(
            "Get internal responses from UPS-Sync with status code {statusCode}",
            LogHelpers.Arguments(response.StatusCode));

        return _logger.ExitMethod(
            await DeserializeSafely<SyncSchedule>(response, _logger, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<SyncSchedule> ChangeScheduleAsync(
        ScheduleRequest schedule,
        string userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        HttpRequestMessage requestMessage =
            RequestBuilder.SetMethod(HttpMethod.Put)
                .SetUri("/Schedule")
                .SetBody(schedule)
                .BuildRequest();

        _logger.LogInfoMessage(
            "Changing schedule from ups sync with the request:{uri} and parameter: isActive : {isActive}",
            LogHelpers.Arguments(requestMessage.RequestUri?.ToString(), schedule.IsActive));

        HttpResponseMessage response = await ClientFactory.CreateClient(SyncConstants.SyncClient)
            .SendAsync(requestMessage, cancellationToken)
            .ConfigureAwait(false);

        CheckRequestMessage(requestMessage);

        _logger.LogInfoMessage(
            "Get internal responses from UPS-Sync with status code {statusCode}",
            LogHelpers.Arguments(response.StatusCode));

        return _logger.ExitMethod(
            await DeserializeSafely<SyncSchedule>(response, _logger, cancellationToken: cancellationToken));
    }
}

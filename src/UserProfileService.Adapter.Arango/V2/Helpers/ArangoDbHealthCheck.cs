using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Administration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Checks the connection to <see cref="ArangoDbHealthCheck" />.
/// </summary>
public class ArangoDbHealthCheck : IHealthCheck
{
    private readonly IArangoDbClientFactory _arangoDbClientFactory;
    private readonly ILogger<ArangoDbHealthCheck> _logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="ArangoDbHealthCheck" />.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="arangoDbClientFactory">The factory that will create <see cref="IArangoDbClient" /> instances.</param>
    public ArangoDbHealthCheck(
        ILogger<ArangoDbHealthCheck> logger,
        IArangoDbClientFactory arangoDbClientFactory)
    {
        _logger = logger;
        _arangoDbClientFactory = arangoDbClientFactory;
    }

    /// <inheritdoc cref="IHealthCheck.CheckHealthAsync" />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _logger.EnterMethod();
        HealthCheckResult checkResult;

        try
        {
            IArangoDbClient client = _arangoDbClientFactory.Create("HealthCheck");
            GetServerVersionResponse response = await client.GetServerVersionAsync();

            if (response.Error)
            {
                checkResult = new HealthCheckResult(
                    context.Registration.FailureStatus,
                    "The connection to ArangoDB could not be established",
                    response.Exception);

                return _logger.ExitMethod(checkResult);
            }

            ServerInfos result = response.Result;

            checkResult = HealthCheckResult.Healthy(
                "Successfully connected to ArangoDB",
                new Dictionary<string, object>
                {
                    { nameof(ServerInfos.Version), result.Version },
                    { nameof(ServerInfos.License), result.License },
                    { nameof(ServerInfos.Server), result.Server }
                });
        }
        catch (OptionsValidationException e)
        {
            _logger.LogWarnMessage(e, "The configuration of ArangoDB is invalid", LogHelpers.Arguments());

            checkResult = new HealthCheckResult(
                context.Registration.FailureStatus,
                "The configuration of ArangoDB is invalid",
                e);
        }
        catch (Exception e)
        {
            _logger.LogWarnMessage(
                e,
                "An error occurred while checking the health of ArangoDB",
                LogHelpers.Arguments());

            checkResult = new HealthCheckResult(
                context.Registration.FailureStatus,
                "The connection to ArangoDB could not be established",
                e);
        }

        return _logger.ExitMethod(checkResult);
    }
}

using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Configuration;

namespace UserProfileService.Services;

/// <summary>
///     Default implementation of <see cref="IDeputyService" />.
/// </summary>
public class DeputyService : IDeputyService
{
    private readonly ProfileDeputyConfiguration _deputyConfiguration;
    private readonly ILogger<DeputyService> _logger;
    private readonly IReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="DeputyService" />.
    /// </summary>
    /// <param name="readService">Read service to retrieve profiles from database.</param>
    /// <param name="options">Options to configure deputy handling.</param>
    /// <param name="logger">The logger.</param>
    public DeputyService(
        IReadService readService,
        IOptions<ProfileDeputyConfiguration> options,
        ILogger<DeputyService> logger)
    {
        _readService = readService;
        _logger = logger;
        _deputyConfiguration = options.Value;
    }

    /// <inheritdoc />
    public async Task<TProfile> GetDeputyOfProfileAsync<TProfile>(
        string profileId,
        RequestedProfileKind expectedKind,
        CancellationToken cancellationToken = default) where TProfile : IProfile
    {
        _logger.EnterMethod();

        if (!_deputyConfiguration.Static)
        {
            throw new NotImplementedException("Deputy handling is only implemented for static configuration.");
        }

        if (expectedKind is RequestedProfileKind.All or RequestedProfileKind.Undefined)
        {
            throw new ArgumentOutOfRangeException(
                $"Deputy handling is not allowed for profile kind of type {RequestedProfileKind.All} or {RequestedProfileKind.Undefined}.");
        }

        try
        {
            var deletedRepoUser =
                await _readService.GetProfileAsync<TProfile>(
                    profileId,
                    expectedKind,
                    cancellationToken: cancellationToken);

            _logger.LogWarnMessage(
                "The requested profile {id} exists in the database. Same profile will be returned.",
                LogHelpers.Arguments(profileId));

            return deletedRepoUser;
        }
        catch (InstanceNotFoundException)
        {
            _logger.LogInfoMessage(
                "The requested profile {id} does not exist in the database. Profile is entitled to have a deputy returned.",
                LogHelpers.Arguments(profileId));
        }

        if (_deputyConfiguration.Profiles.TryGetValue(expectedKind, out string deputyId))
        {
            _logger.LogDebugMessage(
                "Found deputy id {id} for profile kind {kind}.",
                LogHelpers.Arguments(profileId, expectedKind));

            try
            {
                var deputy =
                    await _readService.GetProfileAsync<TProfile>(
                        deputyId,
                        expectedKind,
                        cancellationToken: cancellationToken);

                _logger.LogDebugMessage("Found deputy id {id} in database.", LogHelpers.Arguments(profileId));

                return deputy;
            }
            catch (InstanceNotFoundException e)
            {
                _logger.LogErrorMessage(
                    e,
                    "Could not found deputy with id {id} for profile kind {kind}.",
                    LogHelpers.Arguments(
                        profileId,
                        expectedKind));

                throw;
            }
        }

        throw new ArgumentException($"Deputy handling is not defined for profile kind {expectedKind}.");
    }
}

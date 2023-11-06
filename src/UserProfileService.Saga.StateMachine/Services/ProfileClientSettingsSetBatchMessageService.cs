using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{ProfileClientSettingsSetBatchMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class ProfileClientSettingsSetBatchMessageService : BaseCommandService<ProfileClientSettingsSetBatchMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="ProfileClientSettingsSetBatchMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="ProfileClientSettingsSetBatchMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public ProfileClientSettingsSetBatchMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<ProfileClientSettingsSetBatchMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        ProfileClientSettingsSetBatchMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        List<string> profileIds = message.Resources.Select(r => r.Id).ToList();

        ICollection<IProfile> profiles = await _readService.GetProfilesAsync(
            profileIds,
            ProfileKind.Unknown);

        foreach (ProfileIdent resource in message.Resources)
        {
            IProfile profile = profiles.FirstOrDefault(p => p.Id == resource.Id);

            if (profile == null)
            {
                continue;
            }

            resource.ProfileKind = profile.Kind;
        }

        ProfileClientSettingsSetBatchEvent eventData =
            CreateEvent<ProfileClientSettingsSetBatchEvent, ClientSettingsSetBatchPayload>(
                message,
                correlationId,
                processId,
                initiator);

        return Logger.ExitMethod(eventData);
    }
}

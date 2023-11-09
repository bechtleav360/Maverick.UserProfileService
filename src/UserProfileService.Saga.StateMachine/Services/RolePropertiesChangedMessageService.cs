using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Utilities;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class RolePropertiesChangedMessageService : BaseCommandService<RolePropertiesChangedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="RolePropertiesChangedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="RolePropertiesChangedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public RolePropertiesChangedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<RolePropertiesChangedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<RolePropertiesChangedMessage> ModifyAsync(
        RolePropertiesChangedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        ValidationExtension.RemoveEnumerableNullValues<IList<ExternalIdentifier>, ExternalIdentifier>(
            message.Properties,
            nameof(RoleBasic.ExternalIds),
            true);

        RolePropertiesChangedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        RolePropertiesChangedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        RolePropertiesChangedEvent eventData =
            CreateEvent<RolePropertiesChangedEvent, PropertiesUpdatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        RoleBasic repoRole = await _readService.GetRoleAsync(eventData.Payload.Id);

        eventData.OldRole = repoRole;

        return Logger.ExitMethod(eventData);
    }
}

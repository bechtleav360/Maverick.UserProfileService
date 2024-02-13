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

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{RoleDeletedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class RoleDeletedMessageService : BaseCommandService<RoleDeletedMessage>
{
    private readonly IProjectionReadService _readService;

    /// <summary>
    ///     Create an instance of <see cref="RoleDeletedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="RoleDeletedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="readService">The service to get object from database.</param>
    public RoleDeletedMessageService(
        IValidationService validationService, 
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<RoleDeletedMessageService> logger,
        IProjectionReadService readService) : base(
        validationService,
        logger)
    {
        _readService = readService;
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        RoleDeletedMessage message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        RoleDeletedEvent eventData =
            CreateEvent<RoleDeletedEvent, IdentifierPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        ICollection<Member> assignedMembers =
            await _readService.GetAssignedProfilesAsync(eventData.Payload!.Id);

        RoleBasic? role = await _readService.GetRoleAsync(eventData.Payload.Id);

        eventData.OldRole = role;
        eventData.Profiles = assignedMembers;

        return Logger.ExitMethod(eventData);
    }
}

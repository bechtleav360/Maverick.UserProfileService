using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{GroupCreatedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class GroupCreatedMessageService : BaseCommandService<GroupCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="GroupCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="GroupCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public GroupCreatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<GroupCreatedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<GroupCreatedMessage?> ModifyAsync(
        GroupCreatedMessage? message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();

        if (message != null)
        {
            message.Id = Guid.NewGuid().ToString();

            message.Members ??= Array.Empty<ConditionObjectIdent>();
            message.Tags ??= Array.Empty<TagAssignment>();
            message.ExternalIds ??= new List<ExternalIdentifier>();
            message.ExternalIds = message.ExternalIds.Where(ei => ei != null).ToList();
            message.Members.AddDefaultConditions();
        }

        GroupCreatedMessage? result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        GroupCreatedMessage message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        GroupCreatedEvent eventData =
            CreateEvent<GroupCreatedEvent, GroupCreatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}

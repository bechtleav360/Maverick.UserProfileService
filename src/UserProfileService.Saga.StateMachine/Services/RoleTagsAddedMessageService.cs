using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{RoleTagsAddedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class RoleTagsAddedMessageService : BaseCommandService<RoleTagsAddedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="RoleTagsAddedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="RoleTagsAddedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public RoleTagsAddedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<RoleTagsAddedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<RoleTagsAddedMessage> ModifyAsync(
        RoleTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        message.Tags ??= Array.Empty<TagAssignment>();

        RoleTagsAddedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        RoleTagsAddedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        RoleTagsAddedEvent eventData =
            CreateEvent<RoleTagsAddedEvent, TagsSetPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        Task<IUserProfileServiceEvent> result = Task.FromResult<IUserProfileServiceEvent>(eventData);

        return Logger.ExitMethod(result);
    }
}

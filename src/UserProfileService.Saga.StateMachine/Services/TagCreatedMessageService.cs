using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{TagCreatedMessage}" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class TagCreatedMessageService : BaseCommandService<TagCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="TagCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="TagCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public TagCreatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<TagCreatedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<TagCreatedMessage?> ModifyAsync(
        TagCreatedMessage? message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();

        if (message != null)
        {
            message.Id = Guid.NewGuid().ToString();

            message.ExternalIds ??= new List<ExternalIdentifier>();
            message.ExternalIds = message.ExternalIds.Where(ei => ei != null).ToList();
        }

        TagCreatedMessage? result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        TagCreatedMessage message,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();
        
        TagCreatedEvent eventData =
            CreateEvent<TagCreatedEvent, TagCreatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}

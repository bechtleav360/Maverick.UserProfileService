using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V3;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class UserCreatedMessageService : BaseCommandService<UserCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="UserCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="UserCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public UserCreatedMessageService(
        IValidationService validationService,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor => It is not possible to change the logger
        ILogger<UserCreatedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<UserCreatedMessage> ModifyAsync(
        UserCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        message.Id = Guid.NewGuid().ToString();
        message.Tags ??= Array.Empty<TagAssignment>();
        message.ExternalIds ??= new List<ExternalIdentifier>();
        message.ExternalIds = message.ExternalIds.Where(ei => ei != null).ToList();
        message.Email = message.Email?.ToLowerInvariant();

        UserCreatedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        UserCreatedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        UserCreatedEvent eventData =
            CreateEvent<UserCreatedEvent, UserCreatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}

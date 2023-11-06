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
///     Default implementation for <see cref="ICommandService" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class FunctionTagsAddedMessageService : BaseCommandService<FunctionTagsAddedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="FunctionTagsAddedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="FunctionTagsAddedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public FunctionTagsAddedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<FunctionTagsAddedMessageService> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<FunctionTagsAddedMessage> ModifyAsync(
        FunctionTagsAddedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        message.Tags ??= Array.Empty<TagAssignment>();

        FunctionTagsAddedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        FunctionTagsAddedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        FunctionTagsAddedEvent eventData =
            CreateEvent<FunctionTagsAddedEvent, TagsSetPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        Task<IUserProfileServiceEvent> result = Task.FromResult<IUserProfileServiceEvent>(eventData);

        return Logger.ExitMethod(result);
    }
}

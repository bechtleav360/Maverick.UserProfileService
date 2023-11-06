using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V3;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;

namespace UserProfileService.StateMachine.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class FunctionCreatedMessageService : BaseCommandService<FunctionCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="FunctionCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="FunctionCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public FunctionCreatedMessageService(
        IValidationService validationService,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<FunctionCreatedMessage> logger) : base(
        validationService,
        logger)
    {
    }

    /// <inheritdoc />
    public override async Task<FunctionCreatedMessage> ModifyAsync(
        FunctionCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        message.Id = Guid.NewGuid().ToString();

        message.Tags ??= Array.Empty<TagAssignment>();
        message.ExternalIds ??= new List<ExternalIdentifier>();
        message.ExternalIds = message.ExternalIds.Where(ei => ei != null).ToList();

        FunctionCreatedMessage result = await base.ModifyAsync(message, cancellationToken);

        return Logger.ExitMethod(result);
    }

    /// <inheritdoc />
    public override Task<IUserProfileServiceEvent> CreateAsync(
        FunctionCreatedMessage message,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();

        FunctionCreatedEvent eventData =
            CreateEvent<FunctionCreatedEvent, FunctionCreatedPayload>(
                message,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(Task.FromResult<IUserProfileServiceEvent>(eventData));
    }
}

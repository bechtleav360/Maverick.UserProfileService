using Maverick.UserProfileService.AggregateEvents.Common;
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
///     Default implementation for <see cref="ICommandService" /> where <c>TMessage</c> is of type
///     <see cref="UserSettingObjectUpdatedMessage" />.
/// </summary>
// ReSharper disable once UnusedType.Global => The class will created via reflection.
public class UserSettingObjectUpdatedMessageService : BaseCommandService<UserSettingObjectUpdatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingObjectUpdatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="UserSettingObjectUpdatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public UserSettingObjectUpdatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger in the derived class.
        ILogger<UserSettingObjectUpdatedMessageService> logger) : base(validationService, logger)
    {
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        UserSettingObjectUpdatedMessage createdMessage,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Yield();

        UserSettingObjectUpdatedEvent eventData =
            CreateEvent<UserSettingObjectUpdatedEvent, UserSettingObjectUpdatedPayload>(
                createdMessage,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(eventData);
    }
}

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
///     <see cref="UserSettingsSectionCreatedMessage" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
// ReSharper disable once ClassNeverInstantiated.Global => The class is used with reflection.
public class UserSettingsSectionCreatedMessageService : BaseCommandService<UserSettingsSectionCreatedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingsSectionCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="UserSettingsSectionCreatedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public UserSettingsSectionCreatedMessageService(
        IValidationService validationService,
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<UserSettingsSectionCreatedMessageService> logger) : base(validationService, logger)
    {
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        UserSettingsSectionCreatedMessage createdMessage,
        string correlationId,
        string processId,
        CommandInitiator? initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        await Task.Yield();

        UserSettingsSectionCreatedEvent eventData =
            CreateEvent<UserSettingsSectionCreatedEvent, UserSettingSectionCreatedPayload>(
                createdMessage,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(eventData);
    }
}

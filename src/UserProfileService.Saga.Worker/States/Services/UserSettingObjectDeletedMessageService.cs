using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V3;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Worker.Abstractions;

namespace UserProfileService.Saga.Worker.States.Services;

/// <summary>
///     Default implementation for <see cref="ICommandService{TMessage}" /> where <c>TMessage</c> is of type
///     <see cref="UserSettingObjectDeletedMessage" />.
/// </summary>
// ReSharper disable UnusedType.Global => The class is used with reflection.
public class UserSettingObjectDeletedMessageService : BaseCommandService<UserSettingObjectDeletedMessage>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingsSectionCreatedMessageService" />.
    /// </summary>
    /// <param name="validationService">Service to validate <see cref="UserSettingObjectDeletedMessage" />.</param>
    /// <param name="logger">The logger.</param>
    public UserSettingObjectDeletedMessageService(
        IValidationService validationService, 
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor => The logger can not be changed to ILogger without generic in the derived class.
        ILogger<UserSettingObjectDeletedMessageService> logger) : base(validationService, logger)
    {
    }

    /// <inheritdoc />
    public override async Task<IUserProfileServiceEvent> CreateAsync(
        UserSettingObjectDeletedMessage createdMessage,
        string correlationId,
        string processId,
        CommandInitiator initiator,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        cancellationToken.ThrowIfCancellationRequested();
        
        await Task.Yield();

        UserSettingObjectDeletedEvent eventData =
            CreateEvent<UserSettingObjectDeletedEvent, UserSettingObjectDeletedPayload>(
                createdMessage,
                correlationId,
                processId,
                initiator,
                m => m.Id);

        return Logger.ExitMethod(eventData);
    }
}
